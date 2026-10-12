using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using Electium.Application.DTOs;
using Electium.Domain.Entities;

namespace Electium.Application.Mapping;

public class PoliticalPartyMappingProfile : Profile
{
    public PoliticalPartyMappingProfile()
    {
        CreateMap<PoliticalParty, PoliticalPartyDto>();
        CreateMap<CreatePoliticalPartyDto, PoliticalParty>();
        CreateMap<UpdatePoliticalPartyDto, PoliticalParty>();
    }
}