using AutoMapper;
using Electium.Application.DTOs;
using Electium.Web.ViewModels;

namespace Electium.Web.Mapping;

public class CitizenWebMappingProfile : Profile
{
    public CitizenWebMappingProfile()
    {
        CreateMap<CitizenDto, CitizenListViewModel>();

        CreateMap<CreateCitizenViewModel, CreateCitizenDto>();

        CreateMap<EditCitizenViewModel, UpdateCitizenDto>();
    }
}