using ImmoPilot.Domain.Entities;
using ImmoPilot.Domain.Services;
using ImmoPilot.Domain.ValueObjects;
using Reqnroll;
using Shouldly;

namespace ImmoPilot.BDD.Tests.Steps;

[Binding]
public class DscrQualificationSteps
{
    private Property? _property;
    private FmrData? _fmrData;
    private DscrThresholds _thresholds = DscrThresholds.Default;
    private DscrResult? _result;

    [Given("the default DSCR thresholds")]
    public void GivenDefaultThresholds()
    {
        _thresholds = DscrThresholds.Default;
    }

    [Given("a property priced at {int}")]
    public void GivenPropertyPricedAt(int price)
    {
        _property = new Property
        {
            MlsId = "BDD-001",
            Address = "1 Test St",
            City = "Tempe",
            Zip = "85281",
            Price = price,
            CapRate = 0m,
            CocRate = 0m,
            CashFlow = 0m,
            CashRequired = 0m,
            RehabPercent = 0m
        };
    }

    [Given("the HUD FMR monthly rent is {int}")]
    public void GivenFmrMonthlyRent(int rent)
    {
        _fmrData = new FmrData("85281", rent, 2025, 2);
    }

    [Given("FMR rent is zero")]
    public void GivenFmrRentIsZero()
    {
        _fmrData = new FmrData("85281", 0m, 2025, 2);
    }

    [Given("the down payment is {int} percent")]
    public void GivenDownPaymentPercent(int percent)
    {
        _thresholds = _thresholds with { DownPaymentPercent = percent / 100m };
    }

    [When("the DSCR analysis is performed")]
    public void WhenDscrAnalysisPerformed()
    {
        _property.ShouldNotBeNull("Property must be defined before analysis");
        _fmrData.ShouldNotBeNull("FMR data must be defined before analysis");
        _result = PropertyAnalyzer.Analyze(_property, _fmrData, _thresholds);
    }

    [Then("the property DSCR status is {word}")]
    public void ThenDscrStatusIs(string status)
    {
        _result.ShouldNotBeNull();
        var expected = Enum.Parse<DscrStatus>(status);
        _result.Status.ShouldBe(expected);
    }

    [Then("the DSCR ratio has at most 4 decimal places")]
    public void ThenDscrRatioHasAtMostFourDecimalPlaces()
    {
        _result.ShouldNotBeNull();
        int scale = BitConverter.GetBytes(decimal.GetBits(_result.DscrRatio)[3])[2];
        scale.ShouldBeLessThanOrEqualTo(4);
    }
}
