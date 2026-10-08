using System;
using System.Collections.Generic;
using System.Text;
using Electium.Domain.Entities;

namespace Electium.Application.Interfaces;

public interface ICitizenRepository : IGenericRepository<Citizen>
{
    Task<bool> IdentityNumberExistsAsync(
        string identityNumber,
        int? excludedCitizenId = null,
        CancellationToken cancellationToken = default);
}