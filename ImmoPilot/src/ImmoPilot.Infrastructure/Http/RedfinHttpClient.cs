using System.Globalization;
using ImmoPilot.Application.Interfaces;
using ImmoPilot.Domain.Common;
using ImmoPilot.Domain.Entities;
using ImmoPilot.Domain.ValueObjects;
using ImmoPilot.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ImmoPilot.Infrastructure.Http;

public class RedfinHttpClient(
    HttpClient httpClient,
    IOptions<ApiOptions> apiOptions,
    ILogger<RedfinHttpClient> logger) : IPropertySource
{
    private readonly ApiOptions _options = apiOptions.Value;

    public async Task<Result<IEnumerable<Property>>> FetchListingsAsync(
        MarketConfig config, CancellationToken cancellationToken = default)
    {
        var results = new List<Property>();
        var errors = new List<string>();

        foreach (var zip in config.ZipCodes)
        {
            var result = await FetchByZipAsync(zip, config, cancellationToken);
            if (result.IsSuccess && result.Value is not null)
                results.AddRange(result.Value);
            else
                errors.Add(result.Error ?? $"Unknown error for zip {zip}");
        }

        if (results.Count == 0 && errors.Count > 0)
            return Result<IEnumerable<Property>>.Fail(string.Join("; ", errors));

        return Result<IEnumerable<Property>>.Ok(results);
    }

    private async Task<Result<IEnumerable<Property>>> FetchByZipAsync(
        string zip, MarketConfig config, CancellationToken cancellationToken)
    {
        try
        {
            // Redfin CSV download: filter by zip, sold_within_days, price range
            var url = $"/stingray/api/gis-csv?al=1&market={config.State.ToLower()}"
                    + $"&num_homes=350&ord=redfin-recommended-asc&page_number=1"
                    + $"&region_id={zip}&region_type=2&sf=1,2,3,5,6,7&status=1"
                    + $"&uipt=1,2,3,4,5,6&v=8&max_price={(int)config.MaxPrice}";

            var csv = await httpClient.GetStringAsync(url, cancellationToken);
            var properties = ParseCsv(csv, zip, config);
            return Result<IEnumerable<Property>>.Ok(properties);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch Redfin listings for zip {Zip}", zip);
            return Result<IEnumerable<Property>>.Fail($"REDFIN_HTTP_ERROR({zip}): {ex.Message}");
        }
    }

    private static IEnumerable<Property> ParseCsv(string csv, string zip, MarketConfig config)
    {
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) yield break;

        // Redfin CSV header: MLS#, ADDRESS, CITY, STATE, ZIP, PRICE, ...
        var headers = lines[0].Split(',').Select(h => h.Trim('"').Trim()).ToArray();
        var idx = BuildHeaderIndex(headers);

        foreach (var line in lines.Skip(1))
        {
            var cols = SplitCsvLine(line);
            if (cols.Length < headers.Length) continue;

            if (!TryParseProperty(cols, idx, zip, out var property)) continue;
            if (property is null) continue;
            if (property.Price > config.MaxPrice) continue;

            yield return property;
        }
    }

    private static bool TryParseProperty(string[] cols, Dictionary<string, int> idx, string zip, out Property? property)
    {
        property = null;
        try
        {
            var mlsId = GetCol(cols, idx, "MLS#");
            if (string.IsNullOrWhiteSpace(mlsId)) return false;

            property = new Property
            {
                MlsId = mlsId,
                Address = GetCol(cols, idx, "ADDRESS"),
                City = GetCol(cols, idx, "CITY"),
                Zip = zip,
                Price = ParseDecimal(GetCol(cols, idx, "PRICE")),
                CapRate = 0m,
                CocRate = 0m,
                CashFlow = 0m,
                CashRequired = 0m,
                RehabPercent = 0m
            };
            return true;
        }
        catch { return false; }
    }

    private static Dictionary<string, int> BuildHeaderIndex(string[] headers)
        => headers.Select((h, i) => (h, i)).ToDictionary(x => x.h.ToUpperInvariant(), x => x.i);

    private static string GetCol(string[] cols, Dictionary<string, int> idx, string key)
        => idx.TryGetValue(key, out var i) && i < cols.Length ? cols[i].Trim('"').Trim() : string.Empty;

    private static decimal ParseDecimal(string s)
        => decimal.TryParse(s.Replace("$", "").Replace(",", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : 0m;

    private static string[] SplitCsvLine(string line)
    {
        // Simple CSV split respecting quoted fields
        var result = new List<string>();
        var inQuotes = false;
        var current = new System.Text.StringBuilder();

        foreach (var c in line)
        {
            if (c == '"') { inQuotes = !inQuotes; continue; }
            if (c == ',' && !inQuotes) { result.Add(current.ToString()); current.Clear(); continue; }
            current.Append(c);
        }
        result.Add(current.ToString());
        return [.. result];
    }
}
