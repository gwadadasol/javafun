using ImmoPilot.Domain.Entities;
using ImmoPilot.Domain.Services;
using ImmoPilot.Domain.ValueObjects;
using Shouldly;

namespace ImmoPilot.Domain.Tests;

public class PropertyAnalyzerTests
{
    private static readonly DscrThresholds DefaultThresholds = DscrThresholds.Default;

    private static Property BuildProperty(decimal price = 200_000m) => new()
    {
        MlsId = "TEST-001",
        Address = "123 Main St",
        City = "Tempe",
        Zip = "85281",
        Price = price,
        CapRate = 0m,
        CocRate = 0m,
        CashFlow = 0m,
        CashRequired = 0m,
        RehabPercent = 0m
    };

    private static FmrData BuildFmr(decimal monthlyRent, string zip = "85281") =>
        new(zip, monthlyRent, 2025, 2);

    // ──────────────────────────────────────────────
    // Status classification
    // ──────────────────────────────────────────────

    [Fact]
    public void Analyze_HighRent_ReturnsQualifiedStatus()
    {
        // Price $150k, FMR $2,000/mo → DSCR should be well above 1.25
        var result = PropertyAnalyzer.Analyze(BuildProperty(150_000m), BuildFmr(2_000m), DefaultThresholds);

        result.Status.ShouldBe(DscrStatus.Qualified);
        result.DscrRatio.ShouldBeGreaterThanOrEqualTo(1.25m);
    }

    [Fact]
    public void Analyze_ModerateRent_ReturnsWarningStatus()
    {
        // Price $200k, Down 20% → Loan $160k → Monthly DS ≈ $1,064 → Annual DS ≈ $12,776
        // FMR $2,000/mo → Annual NOI = 2000*12*0.55 = $13,200
        // DSCR ≈ 13,200/12,776 ≈ 1.033 → Warning
        var result = PropertyAnalyzer.Analyze(BuildProperty(200_000m), BuildFmr(2_000m), DefaultThresholds);

        result.Status.ShouldBe(DscrStatus.Warning);
        result.DscrRatio.ShouldBeInRange(1.0m, 1.25m);
    }

    [Fact]
    public void Analyze_LowRent_ReturnsRejectedStatus()
    {
        // Price $500k, FMR $1,000/mo → DSCR < 1.0
        var result = PropertyAnalyzer.Analyze(BuildProperty(500_000m), BuildFmr(1_000m), DefaultThresholds);

        result.Status.ShouldBe(DscrStatus.Rejected);
        result.DscrRatio.ShouldBeLessThan(1.0m);
    }

    // ──────────────────────────────────────────────
    // DSCR formula correctness
    // ──────────────────────────────────────────────

    [Fact]
    public void Analyze_ReturnsExpectedDscrRatio()
    {
        // Manual calculation:
        // Price=200k, Down=20% → Loan=160k
        // Monthly rate = 0.07/12 ≈ 0.005833
        // Debt service = 160k * (0.005833*(1.005833)^360) / ((1.005833)^360 - 1) ≈ $1,064.48/mo
        // Gross rent = 1,500/mo, NOI = 1,500 * 12 * (1 - 0.05 - 0.40) / 12 = 1,500 * 0.55 = 825/mo
        // DSCR = (825*12) / (1,064.48*12) = 825/1,064.48 ≈ 0.7750

        var result = PropertyAnalyzer.Analyze(BuildProperty(200_000m), BuildFmr(1_500m), DefaultThresholds);

        result.MonthlyNoi.ShouldBeInRange(824m, 826m);
        result.MonthlyDebtService.ShouldBeInRange(1_063m, 1_066m);
        result.DscrRatio.ShouldBeInRange(0.755m, 0.795m);
    }

    [Fact]
    public void Analyze_DscrRatioRoundedToFourDecimals()
    {
        var result = PropertyAnalyzer.Analyze(BuildProperty(200_000m), BuildFmr(1_500m), DefaultThresholds);

        int decimals = BitConverter.GetBytes(decimal.GetBits(result.DscrRatio)[3])[2];
        decimals.ShouldBeLessThanOrEqualTo(4);
    }

    // ──────────────────────────────────────────────
    // Edge cases
    // ──────────────────────────────────────────────

    [Fact]
    public void Analyze_ZeroMortgageRate_DoesNotThrow()
    {
        var zeroRateThresholds = DefaultThresholds with { AnnualMortgageRate = 0m };

        Should.NotThrow(() =>
            PropertyAnalyzer.Analyze(BuildProperty(200_000m), BuildFmr(1_500m), zeroRateThresholds));
    }

    [Fact]
    public void Analyze_ZeroPrice_ReturnsRejected()
    {
        var result = PropertyAnalyzer.Analyze(BuildProperty(0m), BuildFmr(1_500m), DefaultThresholds);

        result.Status.ShouldBe(DscrStatus.Rejected);
        result.DscrRatio.ShouldBe(0m);
    }

    [Fact]
    public void Analyze_ZeroFmrRent_ReturnsRejected()
    {
        var result = PropertyAnalyzer.Analyze(BuildProperty(200_000m), BuildFmr(0m), DefaultThresholds);

        result.Status.ShouldBe(DscrStatus.Rejected);
    }

    [Fact]
    public void Analyze_FullDownPayment_NoLoan_ReturnsRejected()
    {
        // 100% down → loan = 0 → debt service = 0 → rejected (guard clause)
        var fullDownThresholds = DefaultThresholds with { DownPaymentPercent = 1.0m };

        var result = PropertyAnalyzer.Analyze(BuildProperty(200_000m), BuildFmr(1_500m), fullDownThresholds);

        result.Status.ShouldBe(DscrStatus.Rejected);
        result.DscrRatio.ShouldBe(0m);
    }

    // ──────────────────────────────────────────────
    // DscrThresholds.Default
    // ──────────────────────────────────────────────

    [Fact]
    public void DscrThresholds_Default_HasExpectedValues()
    {
        DefaultThresholds.MinQualifyingRatio.ShouldBe(1.25m);
        DefaultThresholds.WarningRatio.ShouldBe(1.0m);
        DefaultThresholds.VacancyRate.ShouldBe(0.05m);
        DefaultThresholds.OpexRate.ShouldBe(0.40m);
        DefaultThresholds.AnnualMortgageRate.ShouldBe(0.07m);
        DefaultThresholds.DownPaymentPercent.ShouldBe(0.20m);
    }
}
