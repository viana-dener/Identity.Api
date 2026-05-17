namespace EBL.FIG.Process.Identity.Domain.Interfaces;

public interface IRefreshTokenHasher
{
    byte[] Hash(string refreshToken);
}
