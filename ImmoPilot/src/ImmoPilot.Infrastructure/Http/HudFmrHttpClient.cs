using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ImmoPilot.Application.Interfaces;
using ImmoPilot.Domain.Common;
using ImmoPilot.Domain.Entities;
using ImmoPilot.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ImmoPilot.Infrastructure.Http;

public class HudFmrHttpClient(
    HttpClient httpClient,
    IOptions<ApiOptions> apiOptions,
    ILogger<HudFmrHttpClient> logger) : IHudFmrClient
{
    private readonly ApiOptions _options = apiOptions.Value;

    public async Task<Result<FmrData>> GetFmrByZipAsync(string zip, int bedrooms = 2, CancellationToken cancellationToken = default)
    {
        try
        {
            var bedroomKey = bedrooms switch
            {
                0 => "Efficiency",
                1 => "One-Bedroom",
                2 => "Two-Bedroom",
                3 => "Three-Bedroom",
                _ => "Four-Bedroom"
            };

            var url = $"/api/fmr/statedata?year=2025&zipcode={zip}&token={_options.HudApiKey}";
            var response = await httpClient.GetFromJsonAsync<HudFmrResponse>(url, cancellationToken);

            if (response?.Data?.BasicData is null)
                return Result<FmrData>.Fail($"HUD_NO_DATA: No FMR data for zip {zip}");

            if (!response.Data.BasicData.TryGetValue(bedroomKey, out var rent))
                return Result<FmrData>.Fail($"HUD_NO_BEDROOM: No {bedroomKey} data for zip {zip}");

            return Result<FmrData>.Ok(new FmrData(zip, rent, response.Data.Year ?? 2025, bedrooms));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch HUD FMR for zip {Zip}", zip);
            return Result<FmrData>.Fail($"HUD_HTTP_ERROR: {ex.Message}");
        }
    }

    private record HudFmrResponse(
        [property: JsonPropertyName("data")] HudFmrData? Data);

    private record HudFmrData(
        [property: JsonPropertyName("basicdata")] Dictionary<string, decimal>? BasicData,
        [property: JsonPropertyName("year")] int? Year);
}
