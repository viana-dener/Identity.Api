using AutoMapper;
using EBL.FIG.Process.Identity.Application.Dto.Base;
using EBL.FIG.Process.Identity.Application.Dto.Response.App;
using EBL.FIG.Process.Identity.Domain.Entities;
using EBL.FIG.Process.Identity.Domain.Tools.Pagination;

namespace EBL.FIG.Process.Identity.Application.AutoMapper;

public class AppMappingProfile : Profile
{
    public AppMappingProfile()
    {
        CreateMap<AppEntity, AppResponse>();
        CreateMap<AppEntity, AppDetailResponse>();
        CreateMap<ListPage<AppEntity>, ListPageResponse<AppResponse>>();
    }
}
