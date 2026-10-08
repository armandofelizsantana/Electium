using System;
using System.Collections.Generic;
using System.Text;
using Electium.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Electium.Infrastructure.Configurations;

public class CandidateNominationConfiguration
    : IEntityTypeConfiguration<CandidateNomination>
{
    public void Configure(EntityTypeBuilder<CandidateNomination> builder)
    {
        builder.ToTable("CandidateNominations");

        builder.HasKey(n => n.Id);

        builder.HasOne(n => n.Candidate)
            .WithMany()
            .HasForeignKey(n => n.CandidateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(n => n.PoliticalParty)
            .WithMany()
            .HasForeignKey(n => n.PoliticalPartyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(n => n.ElectivePosition)
            .WithMany()
            .HasForeignKey(n => n.ElectivePositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(n => new
        {
            n.PoliticalPartyId,
            n.ElectivePositionId
        })
            .IsUnique()
            .HasFilter("[IsActive] = 1");

        builder.HasIndex(n => new
        {
            n.PoliticalPartyId,
            n.CandidateId
        })
            .IsUnique()
            .HasFilter("[IsActive] = 1");
    }
}