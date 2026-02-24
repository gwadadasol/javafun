namespace ImmoPilot.Domain.Entities;

public record FmrData(
    string Zip,
    decimal MonthlyRent,
    int Year,
    int Bedrooms = 2
);
