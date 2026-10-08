using System;
using System.Collections.Generic;
using System.Text;
using Electium.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Electium.Infrastructure.Configurations;

public class ElectionParticipationConfiguration
    : IEntityTypeConfiguration<ElectionParticipation>
{
    public void Configure(EntityTypeBuilder<ElectionParticipation> builder)
    {
        builder.ToTable("ElectionParticipations");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.ParticipatedAtUtc)
            .IsRequired();

        builder.HasOne(p => p.Election)
            .WithMany()
            .HasForeignKey(p => p.ElectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Citizen)
            .WithMany()
            .HasForeignKey(p => p.CitizenId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new
        {
            p.ElectionId,
            p.CitizenId
        })
            .IsUnique()
            .HasFilter("[IsActive] = 1");
    }
}