using EBL.FIG.Process.Identity.Domain.Interfaces.Base;

namespace EBL.FIG.Process.Identity.Infra.Data.Context;

public sealed class RequestTenantContext : IRequestTenantContext
{
    private int? _tenantId;

    public int? TenantId => _tenantId;

    public void SetTenantId(int tenantId) => _tenantId = tenantId;

    public void Clear() => _tenantId = null;
}
