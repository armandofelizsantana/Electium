using AutoMapper;
using Electium.Application.DTOs;
using Electium.Web.ViewModels;

namespace Electium.Web.Mapping;

public class ElectionWebMappingProfile : Profile
{
    public ElectionWebMappingProfile()
    {
        CreateMap<ElectionDto, ElectionListViewModel>()
            .ForMember(
                destination => destination.StartsAtLocal,
                options => options.MapFrom(
                    source => source.StartsAtUtc.ToLocalTime()))
            .ForMember(
                destination => destination.EndsAtLocal,
                options => options.MapFrom(
                    source => source.EndsAtUtc.HasValue
                        ? source.EndsAtUtc.Value.ToLocalTime()
                        : (DateTime?)null));

        CreateMap<ElectionDto, EditElectionViewModel>()
            .ForMember(
                destination => destination.StartsAtLocal,
                options => options.MapFrom(
                    source => source.StartsAtUtc.ToLocalTime()))
            .ForMember(
                destination => destination.EndsAtLocal,
                options => options.MapFrom(
                    source => source.EndsAtUtc.HasValue
                        ? source.EndsAtUtc.Value.ToLocalTime()
                        : (DateTime?)null));

        CreateMap<CreateElectionViewModel, CreateElectionDto>()
            .ForMember(
                destination => destination.StartsAtUtc,
                options => options.MapFrom(
                    source => ToUtc(source.StartsAtLocal!.Value)))
            .ForMember(
                destination => destination.EndsAtUtc,
                options => options.MapFrom(
                    source => source.EndsAtLocal.HasValue
                        ? ToUtc(source.EndsAtLocal.Value)
                        : (DateTime?)null));

        CreateMap<EditElectionViewModel, UpdateElectionDto>()
            .ForMember(
                destination => destination.StartsAtUtc,
                options => options.MapFrom(
                    source => ToUtc(source.StartsAtLocal!.Value)))
            .ForMember(
                destination => destination.EndsAtUtc,
                options => options.MapFrom(
                    source => source.EndsAtLocal.HasValue
                        ? ToUtc(source.EndsAtLocal.Value)
                        : (DateTime?)null));
    }

    private static DateTime ToUtc(DateTime localDateTime)
    {
        var local = DateTime.SpecifyKind(
            localDateTime,
            DateTimeKind.Local);

        return local.ToUniversalTime();
    }
}