using System;
using System.Collections.Generic;
using System.Text;
using Electium.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Electium.Infrastructure.Configurations;

public class PoliticalAllianceConfiguration
    : IEntityTypeConfiguration<PoliticalAlliance>
{
    public void Configure(EntityTypeBuilder<PoliticalAlliance> builder)
    {
        builder.ToTable("PoliticalAlliances");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.AcceptedAtUtc)
            .IsRequired();

        builder.HasOne(a => a.FirstParty)
            .WithMany()
            .HasForeignKey(a => a.FirstPartyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.SecondParty)
            .WithMany()
            .HasForeignKey(a => a.SecondPartyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new
        {
            a.FirstPartyId,
            a.SecondPartyId
        })
            .IsUnique()
            .HasFilter("[IsActive] = 1");
    }
}