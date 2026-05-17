using EBL.FIG.Process.Identity.Application.Dto.Result;

namespace EBL.FIG.Process.Identity.Application.Interfaces;

public interface IRefreshTokenService
{
    Task<RefreshTokenIssueResult> IssueAsync(int tenantId, int appId, int userId, CancellationToken ct);
    Task<RefreshTokenRotateResult> RotateAsync(string rawRefreshToken, int tenantId, CancellationToken ct);
    Task<int> RevokeAllAsync(int userId, int tenantId, int revokedBy, CancellationToken ct);
}
