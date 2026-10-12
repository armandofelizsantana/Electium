using AutoMapper;
using Electium.Application.DTOs;
using Electium.Web.ViewModels;

namespace Electium.Web.Mapping;

public class PoliticalPartyWebMappingProfile : Profile
{
    public PoliticalPartyWebMappingProfile()
    {
        CreateMap<PoliticalPartyDto, PoliticalPartyListViewModel>();

        CreateMap<PoliticalPartyDto, EditPoliticalPartyViewModel>()
            .ForMember(
                destination => destination.CurrentLogoPath,
                options => options.MapFrom(source => source.LogoPath));

        CreateMap<CreatePoliticalPartyViewModel, CreatePoliticalPartyDto>()
            .ForMember(
                destination => destination.LogoPath,
                options => options.Ignore());

        CreateMap<EditPoliticalPartyViewModel, UpdatePoliticalPartyDto>()
            .ForMember(
                destination => destination.LogoPath,
                options => options.Ignore());
    }
}