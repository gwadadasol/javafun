namespace ImmoPilot.Domain.ValueObjects;

public record MarketConfig(
    string[] ZipCodes,
    string City,
    string State,
    int BedroomsFilter,
    decimal MaxPrice,
    decimal MinCapRate
);
