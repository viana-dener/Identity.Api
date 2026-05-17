using EBL.FIG.Process.Identity.Domain.Tools.Pagination;

namespace EBL.FIG.Process.Identity.Application.Dto.Base;

public class PagedFilterRequest : Paging
{
    public string Search { get; set; }
    public bool? IsActive { get; set; } = true;
}
