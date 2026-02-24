using ImmoPilot.Domain.Entities;
using ImmoPilot.Domain.ValueObjects;

namespace ImmoPilot.Domain.Services;

public static class PropertyAnalyzer
{
    public static DscrResult Analyze(Property property, FmrData fmrData, DscrThresholds thresholds)
    {
        var annualGrossRent = fmrData.MonthlyRent * 12;
        var annualNoi = annualGrossRent * (1 - thresholds.VacancyRate - thresholds.OpexRate);
        var monthlyNoi = annualNoi / 12;

        var loanAmount = property.Price * (1 - thresholds.DownPaymentPercent);
        var monthlyRate = thresholds.AnnualMortgageRate / 12;
        const int loanTermMonths = 360; // 30-year fixed

        decimal monthlyDebtService;
        if (monthlyRate == 0)
        {
            monthlyDebtService = loanAmount / loanTermMonths;
        }
        else
        {
            monthlyDebtService = loanAmount *
                (monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), loanTermMonths)) /
                ((decimal)Math.Pow((double)(1 + monthlyRate), loanTermMonths) - 1);
        }

        var annualDebtService = monthlyDebtService * 12;

        if (annualDebtService == 0)
            return new DscrResult(DscrStatus.Rejected, 0, monthlyNoi, monthlyDebtService);

        var dscrRatio = annualNoi / annualDebtService;

        var status = dscrRatio >= thresholds.MinQualifyingRatio
            ? DscrStatus.Qualified
            : dscrRatio >= thresholds.WarningRatio
                ? DscrStatus.Warning
                : DscrStatus.Rejected;

        return new DscrResult(status, Math.Round(dscrRatio, 4), monthlyNoi, monthlyDebtService);
    }
}
