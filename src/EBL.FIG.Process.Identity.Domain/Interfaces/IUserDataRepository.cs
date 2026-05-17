using EBL.FIG.Process.Identity.Domain.Entities;
using EBL.FIG.Process.Identity.Domain.ReadModels;
using EBL.FIG.Process.Identity.Domain.Tools.Pagination;

namespace EBL.FIG.Process.Identity.Domain.Interfaces;

public interface IUserDataRepository
{
    Task<IEnumerable<UserEntity>> GetAllAsync(int tenatId, CancellationToken ct);
    Task<UserEntity> GetByIdAsync(int tenatId, int id, CancellationToken ct);
    Task<UserEntity> GetByNormalizedLoginAsync(int tenatId, string email, CancellationToken ct);
    Task<ListPage<UserEntity>> GetPagedAsync(int tenatId, PagedFilter filter, CancellationToken ct);
    Task<bool> ExistsByIdAsync(int tenatId, int id, CancellationToken ct);
    Task<bool> ExistsByNameAsync(int tenatId, string email, CancellationToken ct);
    Task<bool> CreateAsync(UserEntity entity, CancellationToken ct);
    Task<bool> UpdateAsync(UserEntity entity, CancellationToken ct);
    Task<bool> DeleteAsync(UserEntity entity, CancellationToken ct);
}
