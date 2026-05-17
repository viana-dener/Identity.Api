using EBL.FIG.Process.Identity.Application.Dto.Request.RolePermission;
using EBL.FIG.Process.Identity.Application.Dto.Response.RolePermission;
using EBL.FIG.Process.Identity.Domain.ReadModels;
using EBL.FIG.Process.Identity.Domain.Tools.Pagination;
using Microsoft.AspNetCore.Http;

namespace EBL.FIG.Process.Identity.Application.Interfaces;

public interface IRolePermissionAppService
{
    Task<IList<RolePermissionResponse>> GetAllAsync(CancellationToken ct);
    Task<RolePermissionDetailResponse> GetByIdAsync(int id, CancellationToken ct);
    Task<ListPage<RolePermissionResponse>> GetPagedAsync(PagedFilter request, CancellationToken ct);

    Task<RolePermissionResponse> CreateAsync(CreateRolePermissionRequest request, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
    Task<bool> BulkUploadAsync(IFormFile file, CancellationToken ct);
}
