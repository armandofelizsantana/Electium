using AutoMapper;
using Electium.Application.DTOs;
using Electium.Web.ViewModels;

namespace Electium.Web.Mapping;

public class LeaderPartyAssignmentWebMappingProfile : Profile
{
    public LeaderPartyAssignmentWebMappingProfile()
    {
        CreateMap<
            LeaderPartyAssignmentDto,
            LeaderPartyAssignmentListViewModel>();

        CreateMap<
            LeaderPartyAssignmentDto,
            ConfirmRemoveLeaderPartyAssignmentViewModel>();

        CreateMap<
            CreateLeaderPartyAssignmentViewModel,
            CreateLeaderPartyAssignmentDto>();
    }
}