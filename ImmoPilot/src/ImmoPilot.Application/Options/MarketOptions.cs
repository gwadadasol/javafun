namespace ImmoPilot.Application.Options;

public class MarketOptions
{
    public const string SectionName = "Market";
    public string[] ZipCodes { get; set; } = [];
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public int BedroomsFilter { get; set; } = 2;
    public decimal MaxPrice { get; set; } = 500_000m;
    public decimal MinCapRate { get; set; } = 0.05m;
}
