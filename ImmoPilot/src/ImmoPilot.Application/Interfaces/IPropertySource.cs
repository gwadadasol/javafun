using ImmoPilot.Domain.Common;
using ImmoPilot.Domain.Entities;
using ImmoPilot.Domain.ValueObjects;

namespace ImmoPilot.Application.Interfaces;

public interface IPropertySource
{
    Task<Result<IEnumerable<Property>>> FetchListingsAsync(MarketConfig config, CancellationToken cancellationToken = default);
}
