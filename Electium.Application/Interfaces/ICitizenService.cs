using System;
using System.Collections.Generic;
using System.Text;
using Electium.Application.DTOs;
using Electium.Domain.Entities;

namespace Electium.Application.Interfaces;

public interface ICitizenService
    : IGenericService<
        Citizen,
        CitizenDto,
        CreateCitizenDto,
        UpdateCitizenDto>
{
}