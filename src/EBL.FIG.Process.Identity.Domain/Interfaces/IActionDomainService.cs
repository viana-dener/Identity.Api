using EBL.FIG.Process.Identity.Domain.Entities;
using EBL.FIG.Process.Identity.Domain.ReadModels;
using EBL.FIG.Process.Identity.Domain.Tools.Pagination;

namespace EBL.FIG.Process.Identity.Domain.Interfaces;

public interface IActionDomainService
{
    Task<IEnumerable<ActionEntity>> GetAllAsync(int tanantId, int appId, CancellationToken ct);
    Task<ActionEntity> GetByIdAsync(int tanantId, int appId, int id, CancellationToken ct);
    Task<ListPage<ActionEntity>> GetPagedAsync(int tanantId, int appId, PagedFilter request, CancellationToken ct);
    Task<bool> ExistsByNameAsync(int tanantId, int appId, string name, CancellationToken ct);

    Task<bool> CreateAsync(ActionEntity entity, CancellationToken ct);
    Task<bool> UpdateAsync(ActionEntity entity, CancellationToken ct);
    Task<bool> ActivateAsync(ActionEntity entity, CancellationToken ct);
    Task<bool> DeactivateAsync(ActionEntity entity, CancellationToken ct);
    Task<bool> DeleteAsync(ActionEntity entity, CancellationToken ct);
}
