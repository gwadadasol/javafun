namespace ImmoPilot.Application.Options;

public class DscrThresholdOptions
{
    public const string SectionName = "DscrThresholds";
    public decimal MinQualifyingRatio { get; set; } = 1.25m;
    public decimal WarningRatio { get; set; } = 1.0m;
    public decimal VacancyRate { get; set; } = 0.05m;
    public decimal OpexRate { get; set; } = 0.40m;
    public decimal AnnualMortgageRate { get; set; } = 0.07m;
    public decimal DownPaymentPercent { get; set; } = 0.20m;
}
