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

    public async Task DeactivateCandidatesForPositionAsync(
        int electivePositionId,
        CancellationToken cancellationToken = default)
    {
        var activeNominations = await _context.CandidateNominations
            .Where(nomination =>
                nomination.ElectivePositionId == electivePositionId &&
                nomination.IsActive)
            .Include(nomination => nomination.Candidate)
            .ToListAsync(cancellationToken);

        foreach (var nomination in activeNominations)
        {
            // Retira la postulación al puesto.
            nomination.IsActive = false;

            // Inactiva también al candidato asociado.
            nomination.Candidate.IsActive = false;
        }
    }
}