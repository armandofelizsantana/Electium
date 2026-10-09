using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using Electium.Application.DTOs;
using Electium.Domain.Entities;

namespace Electium.Application.Mapping;

public class ElectivePositionMappingProfile : Profile
{
    public ElectivePositionMappingProfile()
    {
        CreateMap<ElectivePosition, ElectivePositionDto>();
        CreateMap<CreateElectivePositionDto, ElectivePosition>();
        CreateMap<UpdateElectivePositionDto, ElectivePosition>();
    }
}