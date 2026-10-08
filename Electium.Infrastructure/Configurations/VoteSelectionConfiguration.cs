using System;
using System.Collections.Generic;
using System.Text;
using Electium.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Electium.Infrastructure.Configurations;

public class VoteSelectionConfiguration
    : IEntityTypeConfiguration<VoteSelection>
{
    public void Configure(EntityTypeBuilder<VoteSelection> builder)
    {
        builder.ToTable("VoteSelections", table =>
        {
            table.HasCheckConstraint(
                "CK_VoteSelection_CandidateOrNone",
                "(([IsNone] = 1 AND [ElectionCandidateId] IS NULL) " +
                "OR ([IsNone] = 0 AND [ElectionCandidateId] IS NOT NULL))");
        });

        builder.HasKey(s => s.Id);

        builder.HasOne(s => s.Ballot)
            .WithMany()
            .HasForeignKey(s => s.BallotId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.ElectivePosition)
            .WithMany()
            .HasForeignKey(s => s.ElectivePositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.ElectionCandidate)
            .WithMany()
            .HasForeignKey(s => s.ElectionCandidateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new
        {
            s.BallotId,
            s.ElectivePositionId
        })
            .IsUnique()
            .HasFilter("[IsActive] = 1");
    }
}