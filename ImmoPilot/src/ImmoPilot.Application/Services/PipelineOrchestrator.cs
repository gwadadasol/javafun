using ImmoPilot.Application.Interfaces;
using ImmoPilot.Application.Models;
using ImmoPilot.Application.Options;
using ImmoPilot.Domain.Entities;
using ImmoPilot.Domain.Services;
using ImmoPilot.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ImmoPilot.Application.Services;

public class PipelineOrchestrator(
    IPropertySource propertySource,
    IHudFmrClient hudFmrClient,
    IPropertyRepository propertyRepository,
    IWhatsAppNotifier whatsAppNotifier,
    IOptions<PipelineOptions> pipelineOptions,
    IOptions<MarketOptions> marketOptions,
    IOptions<DscrThresholdOptions> thresholdOptions,
    ILogger<PipelineOrchestrator> logger)
{
    private readonly PipelineOptions _pipeline = pipelineOptions.Value;
    private readonly MarketOptions _market = marketOptions.Value;
    private readonly DscrThresholdOptions _thresholds = thresholdOptions.Value;

    public async Task<PipelineResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var runId = $"{_pipeline.RunIdPrefix}-{DateTime.UtcNow:yyyyMMdd-HHmm}";
        logger.LogInformation("Pipeline {RunId} started. DryRun={DryRun}", runId, _pipeline.DryRun);

        var marketConfig = BuildMarketConfig();
        var thresholds = BuildThresholds();

        // Step 1: Fetch listings from Redfin
        var fetchResult = await propertySource.FetchListingsAsync(marketConfig, cancellationToken);
        if (!fetchResult.IsSuccess)
        {
            logger.LogError("Fetch failed: {Error}", fetchResult.Error);
            return PipelineResult.Failure;
        }

        var listings = fetchResult.Value!.ToList();
        logger.LogInformation("Fetched {Count} listings from Redfin", listings.Count);

        // Step 2: Enrich with FMR + DSCR, save each property
        var saveErrors = 0;
        foreach (var property in listings)
        {
            await EnrichAndSaveAsync(property, thresholds, cancellationToken);
            var saveResult = await propertyRepository.SaveAsync(property, cancellationToken);
            if (!saveResult.IsSuccess) saveErrors++;
        }

        // Step 3: Get unnotified qualified properties
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var qualifiedResult = await propertyRepository.GetUnnotifiedQualifiedAsync(today, cancellationToken);
        if (!qualifiedResult.IsSuccess)
        {
            logger.LogError("Failed to query qualified properties: {Error}", qualifiedResult.Error);
            return PipelineResult.Failure;
        }

        var qualified = qualifiedResult.Value!.ToList();
        logger.LogInformation("{Count} new qualified properties to notify", qualified.Count);

        // Step 4: Send digest
        if (qualified.Count > 0)
        {
            var notifyResult = await whatsAppNotifier.SendDigestAsync(qualified, runId, cancellationToken);
            if (!notifyResult.IsSuccess)
            {
                logger.LogWarning("Notification failed: {Error}", notifyResult.Error);
                return saveErrors > 0 ? PipelineResult.Failure : PipelineResult.PartialSuccess;
            }

            // Step 5: Log notification (only on real send, not dry-run)
            if (!_pipeline.DryRun)
            {
                var logResult = await propertyRepository.LogNotificationAsync(
                    qualified.Select(p => p.MlsId), today, _market.State, cancellationToken);
                if (!logResult.IsSuccess)
                    logger.LogWarning("Notification log write failed: {Error}", logResult.Error);
            }
        }

        if (saveErrors > 0)
        {
            logger.LogWarning("Pipeline completed with {Count} save errors", saveErrors);
            return PipelineResult.PartialSuccess;
        }

        logger.LogInformation("Pipeline {RunId} completed successfully", runId);
        return PipelineResult.Success;
    }

    private async Task EnrichAndSaveAsync(Property property, DscrThresholds thresholds, CancellationToken ct)
    {
        var fmrResult = await hudFmrClient.GetFmrByZipAsync(property.Zip, _market.BedroomsFilter, ct);

        if (!fmrResult.IsSuccess || fmrResult.Value is null)
        {
            property.FmrStatus = FmrStatus.Unavailable;
            property.DscrStatus = DscrStatus.Rejected;
            logger.LogWarning("No FMR data for {Zip}: {Error}", property.Zip, fmrResult.Error);
            return;
        }

        var fmr = fmrResult.Value;
        property.FmrRent = fmr.MonthlyRent;
        property.FmrYear = fmr.Year;
        property.FmrStatus = FmrStatus.Available;

        var dscrResult = PropertyAnalyzer.Analyze(property, fmr, thresholds);
        property.DscrStatus = dscrResult.Status;
        property.DscrRatio = dscrResult.DscrRatio;

        logger.LogDebug("{MlsId}: DSCR={Ratio:F2} ({Status})", property.MlsId, dscrResult.DscrRatio, dscrResult.Status);
    }

    private MarketConfig BuildMarketConfig() => new(
        _market.ZipCodes, _market.City, _market.State,
        _market.BedroomsFilter, _market.MaxPrice, _market.MinCapRate);

    private DscrThresholds BuildThresholds() => new(
        _thresholds.MinQualifyingRatio, _thresholds.WarningRatio,
        _thresholds.VacancyRate, _thresholds.OpexRate,
        _thresholds.AnnualMortgageRate, _thresholds.DownPaymentPercent);
}
