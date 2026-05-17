using EBL.FIG.Process.Identity.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace EBL.FIG.Process.Identity.Domain.Services;

public class RefreshTokenHasher : IRefreshTokenHasher
{
    public byte[] Hash(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new ArgumentException("Refresh token inválido", nameof(refreshToken));
        }

        return SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
    }
}
