using EBL.FIG.Process.Identity.Domain.Entities;

namespace EBL.FIG.Process.Identity.Application.Dto.Result;

public class RefreshTokenRotateResult
{
    public RefreshTokenEntity OldEntity { get; init; }
    public string NewToken { get; init; }
    public RefreshTokenEntity NewEntity { get; init; }
}
