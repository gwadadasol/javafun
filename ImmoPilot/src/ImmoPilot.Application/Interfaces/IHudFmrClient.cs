using ImmoPilot.Domain.Common;
using ImmoPilot.Domain.Entities;

namespace ImmoPilot.Application.Interfaces;

public interface IHudFmrClient
{
    Task<Result<FmrData>> GetFmrByZipAsync(string zip, int bedrooms = 2, CancellationToken cancellationToken = default);
}
