using EBL.FIG.Process.Identity.Application.Dto.Request.Auth;
using EBL.FIG.Process.Identity.Application.Dto.Response.Auth;

namespace EBL.FIG.Process.Identity.Application.Interfaces;

public interface IAuthAppService
{
    Task<AuthDetailResponse> RegisterAsync(RegisterRequest request, CancellationToken ct);
    Task<AuthDetailResponse> LoginAsync(LoginRequest request, CancellationToken ct);
    Task<AuthDetailResponse> RefreshAsync(RefreshRequest request, CancellationToken ct);
    Task LogoutAsync(RevokeRequest request, CancellationToken ct);
}
