using System;
using System.Collections.Generic;
using System.Text;

namespace Electium.Application.DTOs;

public class CreateElectivePositionDto
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}