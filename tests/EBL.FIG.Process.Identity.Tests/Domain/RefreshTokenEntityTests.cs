using EBL.FIG.Process.Identity.Domain.Entities;

namespace EBL.FIG.Process.Identity.Tests.Domain;

public class RefreshTokenEntityTests
{
    private static byte[] ValidHash => new byte[] { 1, 2, 3, 4, 5 };

    [Fact]
    public void Constructor_ComDadosValidos_DeveCriarEntidade()
    {
        var expires = DateTime.UtcNow.AddDays(7);
        var entity = new RefreshTokenEntity(tenantId: 1, appId: 2, userId: 10, tokenHash: ValidHash, expiresAt: expires, createdBy: 10);

        Assert.Equal(1, entity.TenantId);
        Assert.Equal(2, entity.AppId);
        Assert.Equal(10, entity.UserId);
        Assert.Equal(expires, entity.ExpiresAt);
        Assert.Null(entity.RevokedAt);
    }

    [Theory]
    [InlineData(0, 2, 10)]
    [InlineData(1, 0, 10)]
    [InlineData(1, 2, 0)]
    public void Constructor_ComIdsInvalidos_DeveLancarArgumentException(int tenantId, int appId, int userId)
    {
        Assert.Throws<ArgumentException>(() =>
            new RefreshTokenEntity(tenantId, appId, userId, ValidHash, DateTime.UtcNow.AddDays(1), userId));
    }

    [Fact]
    public void Constructor_ComTokenHashNulo_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new RefreshTokenEntity(1, 2, 10, null, DateTime.UtcNow.AddDays(1), 10));
    }

    [Fact]
    public void IsActive_ComTokenNaoRevogadoENaoExpirado_DeveRetornarTrue()
    {
        var entity = new RefreshTokenEntity(1, 2, 10, ValidHash, DateTime.UtcNow.AddDays(1), 10);
        Assert.True(entity.IsActive());
    }

    [Fact]
    public void IsActive_ComTokenExpirado_DeveRetornarFalse()
    {
        var entity = new RefreshTokenEntity(1, 2, 10, ValidHash, DateTime.UtcNow.AddSeconds(-1), 10);
        Assert.False(entity.IsActive());
    }

    [Fact]
    public void Revoke_DeveDefinirRevokedAtERevokedBy()
    {
        var entity = new RefreshTokenEntity(1, 2, 10, ValidHash, DateTime.UtcNow.AddDays(1), 10);
        entity.Revoke(revokedBy: 99);

        Assert.NotNull(entity.RevokedAt);
        Assert.Equal(99, entity.RevokedBy);
        Assert.False(entity.IsActive());
    }

    [Fact]
    public void IsActive_AposRevoke_DeveRetornarFalse()
    {
        var entity = new RefreshTokenEntity(1, 2, 10, ValidHash, DateTime.UtcNow.AddDays(1), 10);
        entity.Revoke(10);

        Assert.False(entity.IsActive());
    }
}
