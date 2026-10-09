using System;
using System.Collections.Generic;
using System.Text;
using Electium.Application.DTOs;
using Electium.Domain.Entities;

namespace Electium.Application.Interfaces;

public interface IElectivePositionService
    : IGenericService<
        ElectivePosition,
        ElectivePositionDto,
        CreateElectivePositionDto,
        UpdateElectivePositionDto>
{
}