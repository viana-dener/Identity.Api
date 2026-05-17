using EBL.FIG.Process.Identity.Application.Dto.Request.UserRole;
using EBL.FIG.Process.Identity.Application.Dto.Response.UserRole;
using EBL.FIG.Process.Identity.Domain.Entities;
using EBL.FIG.Process.Identity.Domain.ReadModels;
using EBL.FIG.Process.Identity.Domain.Tools.Pagination;
using Microsoft.AspNetCore.Http;

namespace EBL.FIG.Process.Identity.Application.Interfaces;

public interface IUserRoleAppService
{
    Task<IList<UserRoleResponse>> GetAllAsync(CancellationToken ct);
    Task<UserRoleResponse> GetByIdAsync(int id, CancellationToken ct);
    Task<ListPage<UserRoleEntity>> GetPagedAsync(PagedFilter request, CancellationToken ct);

    Task<UserRoleResponse> CreateAsync(CreateUserRoleRequest request, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
    Task<bool> BulkUploadAsync(IFormFile file, CancellationToken ct);
}
