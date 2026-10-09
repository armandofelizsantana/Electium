using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using Electium.Application.DTOs;
using Electium.Domain.Entities;

namespace Electium.Application.Mapping;

public class ElectionMappingProfile : Profile
{
    public ElectionMappingProfile()
    {
        CreateMap<Election, ElectionDto>();

        CreateMap<CreateElectionDto, Election>();

        CreateMap<UpdateElectionDto, Election>();
    }
}