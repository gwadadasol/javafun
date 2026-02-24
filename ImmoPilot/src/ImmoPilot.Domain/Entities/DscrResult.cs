namespace ImmoPilot.Domain.Entities;

public record DscrResult(
    DscrStatus Status,
    decimal DscrRatio,
    decimal MonthlyNoi,
    decimal MonthlyDebtService
);
