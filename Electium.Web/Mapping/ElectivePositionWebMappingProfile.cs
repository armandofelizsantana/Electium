using AutoMapper;
using Electium.Application.DTOs;
using Electium.Web.ViewModels;

namespace Electium.Web.Mapping;

public class ElectivePositionWebMappingProfile : Profile
{
    public ElectivePositionWebMappingProfile()
    {
        CreateMap<ElectivePositionDto, ElectivePositionListViewModel>();

        CreateMap<CreateElectivePositionViewModel, CreateElectivePositionDto>();

        CreateMap<EditElectivePositionViewModel, UpdateElectivePositionDto>();

        CreateMap<ElectivePositionDto, EditElectivePositionViewModel>();
    }
}