namespace ImmoPilot.Domain.ValueObjects;

public record DscrThresholds(
    decimal MinQualifyingRatio,
    decimal WarningRatio,
    decimal VacancyRate,
    decimal OpexRate,
    decimal AnnualMortgageRate,
    decimal DownPaymentPercent
)
{
    public static DscrThresholds Default => new(
        MinQualifyingRatio: 1.25m,
        WarningRatio: 1.0m,
        VacancyRate: 0.05m,
        OpexRate: 0.40m,
        AnnualMortgageRate: 0.07m,
        DownPaymentPercent: 0.20m
    );
}
