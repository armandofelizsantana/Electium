using System;
using System.Collections.Generic;
using System.Text;
using Electium.Application.Interfaces;
using Electium.Domain.Entities;
using Electium.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Electium.Infrastructure.Repositories;

public class CitizenRepository
    : GenericRepository<Citizen>, ICitizenRepository
{
    private readonly ElectiumDbContext _context;

    public CitizenRepository(ElectiumDbContext context)
        : base(context)
    {
        _context = context;
    }

    public Task<bool> IdentityNumberExistsAsync(
        string identityNumber,
        int? excludedCitizenId = null,
        CancellationToken cancellationToken = default)
    {
        return _context.Citizens.AnyAsync(
            citizen =>
                citizen.IdentityNumber == identityNumber &&
                (!excludedCitizenId.HasValue ||
                 citizen.Id != excludedCitizenId.Value),
            cancellationToken);
    }
}