using ImmoPilot.Application.Interfaces;
using ImmoPilot.Application.Models;
using ImmoPilot.Application.Options;
using ImmoPilot.Application.Services;
using ImmoPilot.Domain.Common;
using ImmoPilot.Domain.Entities;
using ImmoPilot.Domain.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using ExtOptions = Microsoft.Extensions.Options.Options;
using NSubstitute;
using Shouldly;

namespace ImmoPilot.Application.Tests;

public class PipelineOrchestratorTests
{
    private readonly IPropertySource _source = Substitute.For<IPropertySource>();
    private readonly IHudFmrClient _hudClient = Substitute.For<IHudFmrClient>();
    private readonly IPropertyRepository _repo = Substitute.For<IPropertyRepository>();
    private readonly IWhatsAppNotifier _notifier = Substitute.For<IWhatsAppNotifier>();

    private PipelineOrchestrator BuildOrchestrator(bool dryRun = true)
    {
        var pipelineOpts = ExtOptions.Create(new PipelineOptions { DryRun = dryRun, RunIdPrefix = "Test" });
        var marketOpts = ExtOptions.Create(new MarketOptions
        {
            ZipCodes = ["85281"],
            City = "Tempe", State = "AZ",
            BedroomsFilter = 2, MaxPrice = 500_000m, MinCapRate = 0.05m
        });
        var thresholdOpts = ExtOptions.Create(new DscrThresholdOptions());

        return new PipelineOrchestrator(
            _source, _hudClient, _repo, _notifier,
            pipelineOpts, marketOpts, thresholdOpts,
            NullLogger<PipelineOrchestrator>.Instance);
    }

    private static Property MakeProperty(string mlsId = "MLS-001") => new()
    {
        MlsId = mlsId, Address = "1 Main St", City = "Tempe", Zip = "85281",
        Price = 200_000m, CapRate = 0m, CocRate = 0m, CashFlow = 0m,
        CashRequired = 0m, RehabPercent = 0m
    };

    [Fact]
    public async Task RunAsync_SourceFails_ReturnsFailure()
    {
        _source.FetchListingsAsync(Arg.Any<MarketConfig>(), Arg.Any<CancellationToken>())
               .Returns(Result<IEnumerable<Property>>.Fail("REDFIN_ERROR"));

        var result = await BuildOrchestrator().RunAsync();

        result.ShouldBe(PipelineResult.Failure);
    }

    [Fact]
    public async Task RunAsync_NoQualifiedProperties_ReturnsSuccess()
    {
        var property = MakeProperty();
        _source.FetchListingsAsync(Arg.Any<MarketConfig>(), Arg.Any<CancellationToken>())
               .Returns(Result<IEnumerable<Property>>.Ok([property]));

        _hudClient.GetFmrByZipAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                  .Returns(Result<FmrData>.Ok(new FmrData("85281", 500m, 2025, 2)));

        _repo.SaveAsync(Arg.Any<Property>(), Arg.Any<CancellationToken>())
             .Returns(Result<bool>.Ok(true));

        _repo.GetUnnotifiedQualifiedAsync(Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
             .Returns(Result<IEnumerable<Property>>.Ok([]));

        var result = await BuildOrchestrator().RunAsync();

        result.ShouldBe(PipelineResult.Success);
        await _notifier.DidNotReceive().SendDigestAsync(Arg.Any<IEnumerable<Property>>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_QualifiedProperties_SendsDigest()
    {
        var property = MakeProperty();
        _source.FetchListingsAsync(Arg.Any<MarketConfig>(), Arg.Any<CancellationToken>())
               .Returns(Result<IEnumerable<Property>>.Ok([property]));

        _hudClient.GetFmrByZipAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                  .Returns(Result<FmrData>.Ok(new FmrData("85281", 3_000m, 2025, 2)));

        _repo.SaveAsync(Arg.Any<Property>(), Arg.Any<CancellationToken>())
             .Returns(Result<bool>.Ok(true));

        _repo.GetUnnotifiedQualifiedAsync(Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
             .Returns(Result<IEnumerable<Property>>.Ok([property]));

        _notifier.SendDigestAsync(Arg.Any<IEnumerable<Property>>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                 .Returns(Result<bool>.Ok(true));

        var result = await BuildOrchestrator(dryRun: true).RunAsync();

        result.ShouldBe(PipelineResult.Success);
        await _notifier.Received(1).SendDigestAsync(Arg.Any<IEnumerable<Property>>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_SaveErrors_ReturnsPartialSuccess()
    {
        var property = MakeProperty();
        _source.FetchListingsAsync(Arg.Any<MarketConfig>(), Arg.Any<CancellationToken>())
               .Returns(Result<IEnumerable<Property>>.Ok([property]));

        _hudClient.GetFmrByZipAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                  .Returns(Result<FmrData>.Ok(new FmrData("85281", 500m, 2025, 2)));

        _repo.SaveAsync(Arg.Any<Property>(), Arg.Any<CancellationToken>())
             .Returns(Result<bool>.Fail("DB_SAVE_ERROR: timeout"));

        _repo.GetUnnotifiedQualifiedAsync(Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
             .Returns(Result<IEnumerable<Property>>.Ok([]));

        var result = await BuildOrchestrator().RunAsync();

        result.ShouldBe(PipelineResult.PartialSuccess);
    }

    [Fact]
    public async Task RunAsync_DryRun_DoesNotCallLogNotification()
    {
        var property = MakeProperty();
        _source.FetchListingsAsync(Arg.Any<MarketConfig>(), Arg.Any<CancellationToken>())
               .Returns(Result<IEnumerable<Property>>.Ok([property]));

        _hudClient.GetFmrByZipAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                  .Returns(Result<FmrData>.Ok(new FmrData("85281", 3_000m, 2025, 2)));

        _repo.SaveAsync(Arg.Any<Property>(), Arg.Any<CancellationToken>())
             .Returns(Result<bool>.Ok(true));

        _repo.GetUnnotifiedQualifiedAsync(Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
             .Returns(Result<IEnumerable<Property>>.Ok([property]));

        _notifier.SendDigestAsync(Arg.Any<IEnumerable<Property>>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                 .Returns(Result<bool>.Ok(true));

        await BuildOrchestrator(dryRun: true).RunAsync();

        await _repo.DidNotReceive().LogNotificationAsync(
            Arg.Any<IEnumerable<string>>(), Arg.Any<DateOnly>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
