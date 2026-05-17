using EBL.FIG.Process.Identity.Domain.Entities;
using EBL.FIG.Process.Identity.Domain.Interfaces;
using EBL.FIG.Process.Identity.Domain.ReadModels;
using EBL.FIG.Process.Identity.Domain.Tools.Pagination;
using EBL.FIG.Process.Identity.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace EBL.FIG.Process.Identity.Infra.Data.Repository;

public class AppDataRepository : IAppDataRepository
{
    private readonly IdentityDbContext _context;

    public AppDataRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<AppEntity> GetByIdAsync(int tenantId, int id, CancellationToken ct)
    {
        return await _context.Set<AppEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id && !x.IsDeleted, ct);
    }

    public async Task<IEnumerable<AppEntity>> GetAllAsync(int tenantId, CancellationToken ct)
    {
        return await _context.Set<AppEntity>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    public async Task<ListPage<AppEntity>> GetPagedAsync(int tenantId, PagedFilter request, CancellationToken ct)
    {
        var query = _context.Set<AppEntity>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(x =>
                EF.Functions.Like(x.Name.ToLower(), $"%{search}%") ||
                EF.Functions.Like(x.Description.ToLower(), $"%{search}%")
            );
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        var count = await query.CountAsync(ct);

        var orderedQuery = CreateSort.ApplyOrdering(query, request);

        var pageNumber = request.PageNumber ?? 1;
        var pageSize = request.PageSize ?? Paging.MinPageSize();

        var result = await orderedQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new ListPage<AppEntity>
        {
            Items = result,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = count,
            TotalPages = (int)Math.Ceiling((double)count / pageSize)
        };
    }

    public async Task<bool> ExistsByIdAsync(int tenantId, int id, CancellationToken ct)
    {
        return await _context.Set<AppEntity>()
            .AsNoTracking()
            .AnyAsync(x => x.TenantId == tenantId && x.Id == id && !x.IsDeleted, ct);
    }

    public async Task<bool> ExistsByNameAsync(int tenantId, string name, CancellationToken ct)
    {
        return await _context.Set<AppEntity>()
            .AsNoTracking()
            .AnyAsync(x => x.TenantId == tenantId && x.Name == name && !x.IsDeleted, ct);
    }

    public async Task<bool> CreateAsync(AppEntity entity, CancellationToken ct)
    {
        await _context.Apps.AddAsync(entity, ct);
        return await _context.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> UpdateAsync(AppEntity entity, CancellationToken ct)
    {
        _context.Apps.Update(entity);
        return await _context.SaveChangesAsync(ct) > 0;
    }
}
