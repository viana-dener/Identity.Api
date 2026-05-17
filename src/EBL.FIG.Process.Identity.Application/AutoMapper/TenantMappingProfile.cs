using AutoMapper;
using EBL.FIG.Process.Identity.Application.Dto.Base;
using EBL.FIG.Process.Identity.Application.Dto.Response.Tenant;
using EBL.FIG.Process.Identity.Domain.Entities;
using EBL.FIG.Process.Identity.Domain.Tools.Pagination;

namespace EBL.FIG.Process.Identity.Application.AutoMapper;

public class TenantMappingProfile : Profile
{
    public TenantMappingProfile()
    {
        CreateMap<TenantEntity, TenantResponse>();
        CreateMap<TenantEntity, TenantDetailResponse>();
        CreateMap<ListPage<TenantEntity>, ListPageResponse<TenantResponse>>();
    }
}
