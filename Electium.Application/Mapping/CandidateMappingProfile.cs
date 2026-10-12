using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using Electium.Application.DTOs;
using Electium.Domain.Entities;

namespace Electium.Application.Mapping;

public class CandidateMappingProfile : Profile
{
    public CandidateMappingProfile()
    {
        CreateMap<Candidate, CandidateDto>();

        CreateMap<CreateCandidateDto, Candidate>();

        CreateMap<UpdateCandidateDto, Candidate>();
    }
}