using AutoMapper;
using Electium.Application.DTOs;
using Electium.Web.ViewModels;

namespace Electium.Web.Mapping;

public class LeaderDashboardWebMappingProfile : Profile
{
    public LeaderDashboardWebMappingProfile()
    {
        CreateMap<PoliticalLeaderDto, LeaderDashboardViewModel>()
            .ForMember(
                destination => destination.FullName,
                options => options.MapFrom(
                    source => $"{source.FirstName} {source.LastName}"))
            .ForMember(
                destination => destination.PoliticalPartyName,
                options => options.Ignore())
            .ForMember(
                destination => destination.PoliticalPartyAcronym,
                options => options.Ignore())
            .ForMember(
                destination => destination.PoliticalPartyLogoPath,
                options => options.Ignore())
            .ForMember(
                destination => destination.ActiveCandidateCount,
                options => options.Ignore())
            .ForMember(
                destination => destination.InactiveCandidateCount,
                options => options.Ignore())
            .ForMember(
                destination => destination.ActiveAllianceCount,
                options => options.Ignore())
            .ForMember(
                destination => destination.PendingIncomingAllianceRequestCount,
                options => options.Ignore())
            .ForMember(
                destination => destination.AssignedCandidateCount,
                options => options.Ignore());

        CreateMap<PoliticalPartyDto, LeaderDashboardViewModel>()
            .ForMember(
                destination => destination.PoliticalPartyName,
                options => options.MapFrom(source => source.Name))
            .ForMember(
                destination => destination.PoliticalPartyAcronym,
                options => options.MapFrom(source => source.Acronym))
            .ForMember(
                destination => destination.PoliticalPartyLogoPath,
                options => options.MapFrom(source => source.LogoPath))
            .ForMember(
                destination => destination.FullName,
                options => options.Ignore())
            .ForMember(
                destination => destination.ActiveCandidateCount,
                options => options.Ignore())
            .ForMember(
                destination => destination.InactiveCandidateCount,
                options => options.Ignore())
            .ForMember(
                destination => destination.ActiveAllianceCount,
                options => options.Ignore())
            .ForMember(
                destination => destination.PendingIncomingAllianceRequestCount,
                options => options.Ignore())
            .ForMember(
                destination => destination.AssignedCandidateCount,
                options => options.Ignore());
    }
}