using EBL.FIG.Process.Identity.Domain.Entities;
using EBL.FIG.Process.Identity.Domain.ReadModels;
using EBL.FIG.Process.Identity.Domain.Tools.Pagination;

namespace EBL.FIG.Process.Identity.Domain.Interfaces;

public interface ITenantDomainService
{
    Task<TenantEntity> GetByIdAsync(int id, CancellationToken ct);
    Task<IEnumerable<TenantEntity>> GetAllAsync(CancellationToken ct);
    Task<ListPage<TenantEntity>> GetPagedAsync(PagedFilter request, CancellationToken ct);
    Task<bool> ExistsByIdAsync(int id, CancellationToken ct);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct);
    Task<bool> CreateAsync(TenantEntity entity, CancellationToken ct);
    Task<bool> UpdateAsync(TenantEntity entity, CancellationToken ct);
    Task<bool> ActivateAsync(TenantEntity entity, CancellationToken ct);
    Task<bool> DeactivateAsync(TenantEntity entity, CancellationToken ct);
    Task<bool> DeleteAsync(TenantEntity entity, CancellationToken ct);
}
