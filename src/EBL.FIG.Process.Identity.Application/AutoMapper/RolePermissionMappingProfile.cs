using AutoMapper;
using EBL.FIG.Process.Identity.Application.Dto.Base;
using EBL.FIG.Process.Identity.Application.Dto.Response.RolePermission;
using EBL.FIG.Process.Identity.Domain.Entities;
using EBL.FIG.Process.Identity.Domain.Tools.Pagination;

namespace EBL.FIG.Process.Identity.Application.AutoMapper;

public class RolePermissionMappingProfile : Profile
{
    public RolePermissionMappingProfile()
    {
        CreateMap<RolePermissionEntity, RolePermissionResponse>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role != null ? src.Role.Name : string.Empty))
            .ForMember(dest => dest.Resource, opt => opt.MapFrom(src => src.Resource != null ? src.Resource.Name : string.Empty))
            .ForMember(dest => dest.Action, opt => opt.MapFrom(src => src.Action != null ? src.Action.Name : string.Empty));

        CreateMap<RolePermissionEntity, RolePermissionDetailResponse>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role != null ? src.Role.Name : string.Empty))
            .ForMember(dest => dest.Resource, opt => opt.MapFrom(src => src.Resource != null ? src.Resource.Name : string.Empty))
            .ForMember(dest => dest.Action, opt => opt.MapFrom(src => src.Action != null ? src.Action.Name : string.Empty));

        CreateMap<ListPage<RolePermissionEntity>, ListPageResponse<RolePermissionResponse>>();
    }
}
