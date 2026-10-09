using System;
using System.Collections.Generic;
using System.Text;
using Electium.Domain.Entities;

namespace Electium.Application.Interfaces;

public interface IElectivePositionRepository
    : IGenericRepository<ElectivePosition>
{
    Task<bool> HasActiveElectionAsync(
        CancellationToken cancellationToken = default);
}