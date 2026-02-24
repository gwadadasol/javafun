using ImmoPilot.Domain.Common;
using ImmoPilot.Domain.Entities;

namespace ImmoPilot.Application.Interfaces;

public interface IWhatsAppNotifier
{
    Task<Result<bool>> SendDigestAsync(IEnumerable<Property> properties, string runId, CancellationToken cancellationToken = default);
}
