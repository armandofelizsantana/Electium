using AutoMapper;
using Electium.Application.DTOs;
using Electium.Web.ViewModels;

namespace Electium.Web.Mapping;

public class VoterBallotWebMappingProfile : Profile
{
    public VoterBallotWebMappingProfile()
    {
        CreateMap<VoterBallotDto, VoterBallotPositionsViewModel>();

        CreateMap<VoterBallotPositionDto, VoterBallotPositionViewModel>();

        CreateMap<VoterBallotPositionDto, VoterPositionViewModel>();

        CreateMap<VoterBallotCandidateDto, VoterBallotCandidateViewModel>();

        CreateMap<VoteReceiptDto, VoteCompletedViewModel>()
            .ForMember(
                destination => destination.SubmittedAtLocal,
                options => options.MapFrom(
                    source => source.SubmittedAtUtc.ToLocalTime()));
    }
}