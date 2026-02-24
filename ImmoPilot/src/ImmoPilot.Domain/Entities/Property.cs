namespace ImmoPilot.Domain.Entities;

public class Property
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string MlsId { get; init; }
    public required string Address { get; init; }
    public required string City { get; init; }
    public required string Zip { get; init; }
    public decimal Price { get; init; }
    public decimal CapRate { get; init; }
    public decimal CocRate { get; init; }
    public decimal CashFlow { get; init; }
    public decimal CashRequired { get; init; }
    public decimal? FmrRent { get; set; }
    public int? FmrYear { get; set; }
    public decimal RehabPercent { get; init; }
    public DscrStatus DscrStatus { get; set; } = DscrStatus.Rejected;
    public FmrStatus FmrStatus { get; set; } = FmrStatus.Unavailable;
    public ListingStatus ListingStatus { get; init; } = ListingStatus.Active;
    public DateTime ScannedAt { get; init; } = DateTime.UtcNow;
    public decimal? DscrRatio { get; set; }
}
