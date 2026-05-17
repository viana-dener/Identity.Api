using EBL.FIG.Process.Identity.Application.Dto.Base;
using EBL.FIG.Process.Identity.Application.Dto.Request.App;
using EBL.FIG.Process.Identity.Application.Dto.Response.App;
using Microsoft.AspNetCore.Http;

namespace EBL.FIG.Process.Identity.Application.Interfaces;

public interface IAppAppService
{
    Task<IEnumerable<AppResponse>> GetAllAsync(CancellationToken ct);
    Task<AppResponse> GetByIdAsync(int id, CancellationToken ct);
    Task<ListPageResponse<AppResponse>> GetPagedAsync(PagedFilterRequest request, CancellationToken ct);
    Task<bool> CreateAsync(CreateAppRequest request, CancellationToken ct);
    Task<bool> UpdateAsync(int id, UpdateAppRequest request, CancellationToken ct);
    Task<bool> ActivateAsync(int id, CancellationToken ct);
    Task<bool> DeactivateAsync(int id, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
    Task<bool> BulkUploadAsync(IFormFile file, CancellationToken ct);
}
