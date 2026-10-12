using System;
using System.Collections.Generic;
using System.Text;
using Electium.Domain.Entities;

namespace Electium.Application.Interfaces;

public interface IPoliticalPartyRepository
    : IGenericRepository<PoliticalParty>
{
    Task<bool> HasActiveElectionAsync(
        CancellationToken cancellationToken = default);

    Task<bool> AcronymExistsAsync(
        string acronym,
        int? excludedPartyId = null,
        CancellationToken cancellationToken = default);

    Task DeactivateRelatedDataAsync(
        int politicalPartyId,
        CancellationToken cancellationToken = default);
}