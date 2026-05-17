using AutoMapper;
using EBL.FIG.Process.Identity.Application.Dto.Base;
using EBL.FIG.Process.Identity.Application.Dto.Response.UserRole;
using EBL.FIG.Process.Identity.Domain.Entities;
using EBL.FIG.Process.Identity.Domain.Tools.Pagination;

namespace EBL.FIG.Process.Identity.Application.AutoMapper;

public class UserRoleMappingProfile : Profile
{
    public UserRoleMappingProfile()
    {
        CreateMap<UserRoleEntity, UserRoleResponse>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.Name : string.Empty))
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.Name : string.Empty));

        CreateMap<UserRoleEntity, UserRoleDetailResponse>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.Name : string.Empty))
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.Name : string.Empty));

        CreateMap<ListPage<UserRoleEntity>, ListPageResponse<UserRoleResponse>>();
    }
}