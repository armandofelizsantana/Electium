using System;
using System.Collections.Generic;
using System.Text;
using Electium.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Electium.Infrastructure.Configurations;

public class ElectionCandidateConfiguration
    : IEntityTypeConfiguration<ElectionCandidate>
{
    public void Configure(EntityTypeBuilder<ElectionCandidate> builder)
    {
        builder.ToTable("ElectionCandidates");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CandidateFirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.CandidateLastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.CandidatePhotoPath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.PartyName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.PartyAcronym)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.PartyLogoPath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.PositionName)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasOne(c => c.Election)
            .WithMany()
            .HasForeignKey(c => c.ElectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new
        {
            c.ElectionId,
            c.PoliticalPartyId,
            c.ElectivePositionId
        })
            .IsUnique()
            .HasFilter("[IsActive] = 1");
    }
}