using AutoMapper;
using Electium.Application.DTOs;
using Electium.Web.ViewModels;

namespace Electium.Web.Mapping;

public class CandidateWebMappingProfile : Profile
{
    public CandidateWebMappingProfile()
    {
        CreateMap<CandidateDto, CandidateListItemViewModel>();

        CreateMap<CandidateDto, EditCandidateViewModel>()
            .ForMember(
                destination => destination.CurrentPhotoPath,
                options => options.MapFrom(source => source.PhotoPath));

        CreateMap<CreateCandidateViewModel, CreateCandidateDto>()
            .ForMember(
                destination => destination.PhotoPath,
                options => options.Ignore());

        CreateMap<EditCandidateViewModel, UpdateCandidateDto>()
            .ForMember(
                destination => destination.PhotoPath,
                options => options.Ignore());
    }
}