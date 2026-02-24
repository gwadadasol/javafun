using ImmoPilot.Domain.Entities;

namespace ImmoPilot.Infrastructure.Persistence.Entities;

public class PropertyEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string MlsId { get; set; }
    public required string Address { get; set; }
    public required string City { get; set; }
    public required string Zip { get; set; }
    public decimal Price { get; set; }
    public decimal CapRate { get; set; }
    public decimal CocRate { get; set; }
    public decimal CashFlow { get; set; }
    public decimal CashRequired { get; set; }
    public decimal? FmrRent { get; set; }
    public int? FmrYear { get; set; }
    public decimal RehabPercent { get; set; }
    public DscrStatus DscrStatus { get; set; } = DscrStatus.Rejected;
    public FmrStatus FmrStatus { get; set; } = FmrStatus.Unavailable;
    public ListingStatus ListingStatus { get; set; } = ListingStatus.Active;
    public DateTime ScannedAt { get; set; } = DateTime.UtcNow;
    public decimal? DscrRatio { get; set; }

    public ICollection<NotificationLogEntity> NotificationLogs { get; set; } = [];
}
