using ImmoPilot.Domain.Common;
using ImmoPilot.Domain.Entities;

namespace ImmoPilot.Application.Interfaces;

public interface IPropertyRepository
{
    Task<Result<bool>> SaveAsync(Property property, CancellationToken cancellationToken = default);
    Task<bool> IsAlreadyNotifiedAsync(string mlsId, DateOnly date, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<Property>>> GetUnnotifiedQualifiedAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task<Result<Property?>> GetByMlsIdAsync(string mlsId, CancellationToken cancellationToken = default);
    Task<Result<bool>> LogNotificationAsync(IEnumerable<string> mlsIds, DateOnly date, string recipients, CancellationToken cancellationToken = default);
}
