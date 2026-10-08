using System;
using System.Collections.Generic;
using System.Text;
using Electium.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Electium.Infrastructure.Persistence
{
    public class ElectiumDbContext : DbContext
    {
        public ElectiumDbContext(DbContextOptions<ElectiumDbContext> options)
            : base(options)
        {
        }

        public DbSet<Citizen> Citizens => Set<Citizen>();

        public DbSet<PoliticalParty> PoliticalParties => Set<PoliticalParty>();

        public DbSet<ElectivePosition> ElectivePositions => Set<ElectivePosition>();

        public DbSet<Candidate> Candidates => Set<Candidate>();

        public DbSet<Election> Elections => Set<Election>();

        public DbSet<CandidateNomination> CandidateNominations =>
            Set<CandidateNomination>();

        public DbSet<ElectionCandidate> ElectionCandidates =>
            Set<ElectionCandidate>();

        public DbSet<AllianceRequest> AllianceRequests =>
            Set<AllianceRequest>();

        public DbSet<PoliticalAlliance> PoliticalAlliances =>
            Set<PoliticalAlliance>();

        public DbSet<ElectionParticipation> ElectionParticipations =>
            Set<ElectionParticipation>();

        public DbSet<Ballot> Ballots => Set<Ballot>();

        public DbSet<VoteSelection> VoteSelections => Set<VoteSelection>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(ElectiumDbContext).Assembly);
        }
    }
}
    