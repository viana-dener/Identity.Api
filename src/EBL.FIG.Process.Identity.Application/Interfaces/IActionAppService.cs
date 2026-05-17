using Microsoft.AspNetCore.Http;
using EBL.FIG.Process.Identity.Application.Dto.Base;
using EBL.FIG.Process.Identity.Application.Dto.Request.Action;
using EBL.FIG.Process.Identity.Application.Dto.Response.Action;

namespace EBL.FIG.Process.Identity.Application.Interfaces;

public interface IActionAppService
{
    Task<IEnumerable<ActionResponse>> GetAllAsync(CancellationToken ct);
    Task<ActionResponse> GetByIdAsync(int id, CancellationToken ct);
    Task<ListPageResponse<ActionResponse>> GetPagedAsync(PagedFilterRequest request, CancellationToken ct);
    Task<bool> CreateAsync(CreateActionRequest request, CancellationToken ct);
    Task<bool> UpdateAsync(int id, UpdateActionRequest request, CancellationToken ct);
    Task<bool> ActivateAsync(int id, CancellationToken ct);
    Task<bool> DeactivateAsync(int id, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
    Task<bool> BulkUploadAsync(IFormFile file, CancellationToken ct);
}
