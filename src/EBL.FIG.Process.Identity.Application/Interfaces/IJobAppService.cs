using EBL.FIG.Process.Identity.Application.Dto.Base;
using EBL.FIG.Process.Identity.Application.Dto.Request.Job;
using EBL.FIG.Process.Identity.Application.Dto.Response.Job;
using EBL.FIG.Process.Identity.Domain.ReadModels;

namespace EBL.FIG.Process.Identity.Application.Interfaces;

public interface IJobAppService
{
    Task<IEnumerable<JobResponse>> GetAllAsync(CancellationToken ct);
    Task<JobDetailResponse> GetByIdAsync(int id, CancellationToken ct);
    Task<ListPageResponse<JobResponse>> GetPagedAsync(JobPagedFilter request, CancellationToken ct);
    Task<bool> CreateAsync(CreateJobRequest request, CancellationToken ct);
    Task<bool> UpdateAsync(int id, UpdateJobRequest request, CancellationToken ct);
    Task<bool> ActivateAsync(int id, CancellationToken ct);
    Task<bool> DeactivateAsync(int id, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
    Task<bool> ExecuteAsync(int id, CancellationToken ct);
}
