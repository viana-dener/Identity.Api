using AutoMapper;
using EBL.FIG.Process.Identity.Application.Dto.Base;
using EBL.FIG.Process.Identity.Application.Dto.Response.User;
using EBL.FIG.Process.Identity.Domain.Entities;
using EBL.FIG.Process.Identity.Domain.Tools.Pagination;

namespace EBL.FIG.Process.Identity.Application.AutoMapper;

/// <summary>
/// Perfil de mapeamento do AutoMapper para UserEntity
/// </summary>
public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        // Mapeia UserEntity -> UserResponse
        CreateMap<UserEntity, UserResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.LastAccessAt, opt => opt.MapFrom(src => src.LastAccessAt))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

        CreateMap<UserEntity, UserDetailResponse>()
             .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
             .ForMember(dest => dest.TenantId, opt => opt.MapFrom(src => src.TenantId))
             .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
             .ForMember(dest => dest.LastAccessAt, opt => opt.MapFrom(src => src.LastAccessAt))
             .ForMember(dest => dest.UrlImage, opt => opt.MapFrom(src => src.UrlImage))
             .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

        CreateMap<ListPage<UserEntity>, ListPageResponse<UserResponse>>();
    }
}
