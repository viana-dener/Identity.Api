using EBL.FIG.Process.Identity.Domain.Entities;

namespace EBL.FIG.Process.Identity.Application.Dto.Result;

public class RefreshTokenIssueResult
{
    public string Token { get; init; }
    public DateTime ExpiresAt { get; init; }
    public RefreshTokenEntity Entity { get; init; }
}
