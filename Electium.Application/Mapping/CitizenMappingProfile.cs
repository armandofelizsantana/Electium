using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using Electium.Application.DTOs;
using Electium.Domain.Entities;

namespace Electium.Application.Mapping;

public class CitizenMappingProfile : Profile
{
    public CitizenMappingProfile()
    {
        CreateMap<Citizen, CitizenDto>();

        CreateMap<CreateCitizenDto, Citizen>();

        CreateMap<UpdateCitizenDto, Citizen>();
    }
}