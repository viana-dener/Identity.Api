using EBL.FIG.Process.Identity.Domain.Tools.Pagination;

namespace EBL.FIG.Process.Identity.Domain.ReadModels;

public class PagedFilter : Paging
{
    public string Search { get; set; }
    public bool? IsActive { get; set; } = true;
    public PagedFilter(string search, bool? isActive, int? pageNumber, int? pageSize, string sortBy, string sortDirection)
    {
        Search = search;
        IsActive = isActive;
        PageNumber = pageNumber;
        PageSize = pageSize;
        SortBy = sortBy;
        SortDirection = sortDirection;
    }
}
