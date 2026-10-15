using AutoMapper;
using Electium.Application.DTOs;
using Electium.Web.ViewModels;

namespace Electium.Web.Mapping;

public class ElectionWebMappingProfile : Profile
{
    public ElectionWebMappingProfile()
    {
        CreateMap<ElectionSummaryDto, ElectionListViewModel>()
            .ForMember(
                destination => destination.ElectionDateLocal,
                options => options.MapFrom(
                    source => source.StartsAtUtc.ToLocalTime().Date));

        CreateMap<CreateElectionViewModel, CreateElectionDto>()
            .ForMember(
                destination => destination.Year,
                options => options.MapFrom(
                    source => source.ElectionDateLocal!.Value.Year))
            .ForMember(
                destination => destination.StartsAtUtc,
                options => options.MapFrom(
                    source => ToUtc(source.ElectionDateLocal!.Value.Date)))
            .ForMember(
                destination => destination.EndsAtUtc,
                options => options.Ignore());

        CreateMap<ElectionDto, ConfirmCompleteElectionViewModel>();
        CreateMap<ElectionResultsDto, ElectionResultsViewModel>();
        CreateMap<ElectionPositionResultDto, ElectionPositionResultViewModel>();
        CreateMap<ElectionCandidateResultDto, ElectionCandidateResultViewModel>();
        CreateMap<ElectionSummaryDto, AdminElectionSummaryViewModel>();
    }

    private static DateTime ToUtc(DateTime localDate)
    {
        var local = DateTime.SpecifyKind(
            localDate.Date,
            DateTimeKind.Local);

        return local.ToUniversalTime();
    }
}
