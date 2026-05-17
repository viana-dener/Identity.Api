using EBL.FIG.Process.Identity.Domain.Entities;

namespace EBL.FIG.Process.Identity.Tests.Domain;

public class PasswordResetTokenEntityTests
{
    private static byte[] ValidHash => new byte[] { 1, 2, 3, 4, 5 };
    private static DateTime FutureExpiry => DateTime.UtcNow.AddMinutes(15);

    [Fact]
    public void Constructor_ComDadosValidos_DeveCriarEntidade()
    {
        var expires = FutureExpiry;
        var entity = new PasswordResetTokenEntity(tenantId: 1, userId: 10, tokenHash: ValidHash, expiresAt: expires, createdBy: 0);

        Assert.Equal(1, entity.TenantId);
        Assert.Equal(10, entity.UserId);
        Assert.Equal(ValidHash, entity.TokenHash);
        Assert.Equal(expires, entity.ExpiresAt);
        Assert.False(entity.Used);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    public void Constructor_ComIdsInvalidos_DeveLancarArgumentException(int tenantId, int userId)
    {
        Assert.Throws<ArgumentException>(() =>
            new PasswordResetTokenEntity(tenantId, userId, ValidHash, FutureExpiry, 0));
    }

    [Fact]
    public void Constructor_ComTokenHashNulo_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new PasswordResetTokenEntity(1, 10, null!, FutureExpiry, 0));
    }

    [Fact]
    public void Constructor_ComExpiresAtNoPassado_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new PasswordResetTokenEntity(1, 10, ValidHash, DateTime.UtcNow.AddSeconds(-1), 0));
    }

    [Fact]
    public void IsValid_ComTokenNaoUsadoENaoExpirado_DeveRetornarTrue()
    {
        var entity = new PasswordResetTokenEntity(1, 10, ValidHash, FutureExpiry, 0);
        Assert.True(entity.IsValid());
    }

    [Fact]
    public void IsValid_AposMarkAsUsed_DeveRetornarFalse()
    {
        var entity = new PasswordResetTokenEntity(1, 10, ValidHash, FutureExpiry, 0);
        entity.MarkAsUsed(modifiedBy: 99);

        Assert.False(entity.IsValid());
    }

    [Fact]
    public void MarkAsUsed_DeveDefinirUsedEModifiedBy()
    {
        var entity = new PasswordResetTokenEntity(1, 10, ValidHash, FutureExpiry, 0);
        entity.MarkAsUsed(modifiedBy: 99);

        Assert.True(entity.Used);
        Assert.Equal(99, entity.ModifiedBy);
        Assert.NotNull(entity.ModifiedAt);
    }
}
