using ImmoPilot.Application.Interfaces;
using ImmoPilot.Domain.Common;
using ImmoPilot.Domain.Entities;
using ImmoPilot.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ImmoPilot.Infrastructure.Persistence;

public class EfPropertyRepository(ImmoPilotDbContext dbContext, ILogger<EfPropertyRepository> logger)
    : IPropertyRepository
{
    public async Task<Result<bool>> SaveAsync(Property property, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await dbContext.Properties
                .FirstOrDefaultAsync(p => p.MlsId == property.MlsId, cancellationToken);

            if (existing is null)
            {
                var entity = MapToEntity(property);
                dbContext.Properties.Add(entity);
            }
            else
            {
                existing.DscrStatus = property.DscrStatus;
                existing.FmrStatus = property.FmrStatus;
                existing.FmrRent = property.FmrRent;
                existing.FmrYear = property.FmrYear;
                existing.DscrRatio = property.DscrRatio;
                existing.ScannedAt = property.ScannedAt;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save property {MlsId}", property.MlsId);
            return Result<bool>.Fail($"DB_SAVE_ERROR: {ex.Message}");
        }
    }

    public async Task<bool> IsAlreadyNotifiedAsync(string mlsId, DateOnly date, CancellationToken cancellationToken = default)
    {
        return await dbContext.NotificationLogs
            .AnyAsync(n => n.Property.MlsId == mlsId && n.DigestDate == date, cancellationToken);
    }

    public async Task<Result<IEnumerable<Property>>> GetUnnotifiedQualifiedAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await dbContext.Properties
                .Where(p => (p.DscrStatus == DscrStatus.Qualified || p.DscrStatus == DscrStatus.Warning)
                         && !p.NotificationLogs.Any(n => n.DigestDate == date))
                .ToListAsync(cancellationToken);

            return Result<IEnumerable<Property>>.Ok(entities.Select(MapToDomain));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get unnotified qualified properties for {Date}", date);
            return Result<IEnumerable<Property>>.Fail($"DB_QUERY_ERROR: {ex.Message}");
        }
    }

    public async Task<Result<Property?>> GetByMlsIdAsync(string mlsId, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await dbContext.Properties
                .FirstOrDefaultAsync(p => p.MlsId == mlsId, cancellationToken);

            return Result<Property?>.Ok(entity is null ? null : MapToDomain(entity));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get property {MlsId}", mlsId);
            return Result<Property?>.Fail($"DB_QUERY_ERROR: {ex.Message}");
        }
    }

    public async Task<Result<bool>> LogNotificationAsync(IEnumerable<string> mlsIds, DateOnly date, string recipients, CancellationToken cancellationToken = default)
    {
        try
        {
            var mlsIdList = mlsIds.ToList();
            var propertyIds = await dbContext.Properties
                .Where(p => mlsIdList.Contains(p.MlsId))
                .Select(p => new { p.Id, p.MlsId })
                .ToListAsync(cancellationToken);

            var logs = propertyIds.Select(p => new NotificationLogEntity
            {
                PropertyId = p.Id,
                DigestDate = date,
                Recipients = recipients
            });

            dbContext.NotificationLogs.AddRange(logs);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to log notifications for {Date}", date);
            return Result<bool>.Fail($"DB_LOG_ERROR: {ex.Message}");
        }
    }

    private static PropertyEntity MapToEntity(Property p) => new()
    {
        Id = p.Id,
        MlsId = p.MlsId,
        Address = p.Address,
        City = p.City,
        Zip = p.Zip,
        Price = p.Price,
        CapRate = p.CapRate,
        CocRate = p.CocRate,
        CashFlow = p.CashFlow,
        CashRequired = p.CashRequired,
        FmrRent = p.FmrRent,
        FmrYear = p.FmrYear,
        RehabPercent = p.RehabPercent,
        DscrStatus = p.DscrStatus,
        FmrStatus = p.FmrStatus,
        ListingStatus = p.ListingStatus,
        ScannedAt = p.ScannedAt,
        DscrRatio = p.DscrRatio
    };

    private static Property MapToDomain(PropertyEntity e) => new()
    {
        Id = e.Id,
        MlsId = e.MlsId,
        Address = e.Address,
        City = e.City,
        Zip = e.Zip,
        Price = e.Price,
        CapRate = e.CapRate,
        CocRate = e.CocRate,
        CashFlow = e.CashFlow,
        CashRequired = e.CashRequired,
        FmrRent = e.FmrRent,
        FmrYear = e.FmrYear,
        RehabPercent = e.RehabPercent,
        DscrStatus = e.DscrStatus,
        FmrStatus = e.FmrStatus,
        ListingStatus = e.ListingStatus,
        ScannedAt = e.ScannedAt,
        DscrRatio = e.DscrRatio
    };
}
