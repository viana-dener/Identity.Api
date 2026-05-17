using EBL.FIG.Process.Identity.Domain.Entities;
using EBL.FIG.Process.Identity.Domain.Interfaces;
using EBL.FIG.Process.Identity.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace EBL.FIG.Process.Identity.Infra.Data.Repository;

public class RefreshTokenDataRepository : IRefreshTokenDataRepository
{
    private readonly IdentityDbContext _context;

    public RefreshTokenDataRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshTokenEntity> GetByTokenHashAsync(byte[] tokenHash, int tenantId, CancellationToken ct)
    {
        return await _context.Set<RefreshTokenEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash && x.TenantId == tenantId, ct);
    }

    public async Task<IEnumerable<RefreshTokenEntity>> GetByUserAsync(int userId, int tenantId, CancellationToken ct)
    {
        return await _context.Set<RefreshTokenEntity>()
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.TenantId == tenantId)
            .ToListAsync(ct);
    }

    public async Task<bool> CreateAsync(RefreshTokenEntity entity, CancellationToken ct)
    {
        await _context.RefreshTokens.AddAsync(entity, ct);
        return await _context.SaveChangesAsync(ct) > 0;
    }
    public async Task<bool> RevokeAsync(RefreshTokenEntity entity, CancellationToken ct)
    {
        _context.RefreshTokens.Update(entity);
        return await _context.SaveChangesAsync(ct) > 0;
    }
}
