using System;
using System.Collections.Generic;
using System.Text;
using Electium.Application.Interfaces;
using Electium.Domain.Entities;
using Electium.Domain.Enums;
using Electium.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Electium.Infrastructure.Repositories;

public class ElectivePositionRepository
    : GenericRepository<ElectivePosition>, IElectivePositionRepository
{
    private readonly ElectiumDbContext _context;

    public ElectivePositionRepository(ElectiumDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<bool> HasActiveElectionAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Elections
            .AsNoTracking()
            .AnyAsync(
                election =>
                    election.IsActive &&
                    election.Status == ElectionStatus.Active,
                cancellationToken);
    }
}