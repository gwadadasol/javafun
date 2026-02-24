using ImmoPilot.Application.Interfaces;
using ImmoPilot.Domain.Common;
using ImmoPilot.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ImmoPilot.Infrastructure.Notifications;

/// <summary>
/// No-op notifier used in dry-run mode. Logs what would be sent.
/// </summary>
public class NullWhatsAppNotifier(ILogger<NullWhatsAppNotifier> logger) : IWhatsAppNotifier
{
    public Task<Result<bool>> SendDigestAsync(
        IEnumerable<Property> properties, string runId, CancellationToken cancellationToken = default)
    {
        var props = properties.ToList();
        logger.LogInformation("[DRY-RUN] {RunId}: Would send {Count} properties via WhatsApp.", runId, props.Count);

        foreach (var p in props)
            logger.LogInformation("[DRY-RUN]  - {Address} | DSCR={Ratio:F2} | ${Price:N0}", p.Address, p.DscrRatio, p.Price);

        return Task.FromResult(Result<bool>.Ok(true));
    }
}
