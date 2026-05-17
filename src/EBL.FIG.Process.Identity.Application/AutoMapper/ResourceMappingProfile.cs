using AutoMapper;
using EBL.FIG.Process.Identity.Application.Dto.Base;
using EBL.FIG.Process.Identity.Application.Dto.Response.Resource;
using EBL.FIG.Process.Identity.Domain.Entities;
using EBL.FIG.Process.Identity.Domain.Tools.Pagination;

namespace EBL.FIG.Process.Identity.Application.AutoMapper;

/// <summary>
/// Perfil de mapeamento do AutoMapper para ResourceEntity
/// </summary>
public class ResourceMappingProfile : Profile
{
    public ResourceMappingProfile()
    {
        // Mapeia ResourceEntity -> ResourceResponse
        CreateMap<ResourceEntity, ResourceResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

        CreateMap<ResourceEntity, ResourceDetailResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

        CreateMap<ListPage<ResourceEntity>, ListPageResponse<ResourceResponse>>();
    }
}
