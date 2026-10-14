using AutoMapper;
using Electium.Application.DTOs;
using Electium.Web.ViewModels;

namespace Electium.Web.Mapping;

public class LeaderAllianceWebMappingProfile : Profile
{
    public LeaderAllianceWebMappingProfile()
    {
        CreateMap<LeaderAllianceDashboardDto, LeaderAllianceDashboardViewModel>();

        CreateMap<
            IncomingAllianceRequestDto,
            IncomingAllianceRequestViewModel>();

        CreateMap<
            OutgoingAllianceRequestDto,
            OutgoingAllianceRequestViewModel>();

        CreateMap<ActiveAllianceDto, ActiveAllianceViewModel>();
    }
}