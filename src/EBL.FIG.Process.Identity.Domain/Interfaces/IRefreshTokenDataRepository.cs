using EBL.FIG.Process.Identity.Domain.Entities;

namespace EBL.FIG.Process.Identity.Domain.Interfaces;

public interface IRefreshTokenDataRepository
{
    Task<RefreshTokenEntity> GetByTokenHashAsync(byte[] tokenHash, int tenantId, CancellationToken ct);
    Task<IEnumerable<RefreshTokenEntity>> GetByUserAsync(int userId, int tenantId, CancellationToken ct);
    Task<bool> CreateAsync(RefreshTokenEntity entity, CancellationToken ct);
    Task<bool> RevokeAsync(RefreshTokenEntity entity, CancellationToken ct);
}
