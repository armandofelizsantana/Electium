using System;
using System.Collections.Generic;
using System.Text;
using Electium.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Electium.Infrastructure.Configurations;

public class AllianceRequestConfiguration
    : IEntityTypeConfiguration<AllianceRequest>
{
    public void Configure(EntityTypeBuilder<AllianceRequest> builder)
    {
        builder.ToTable("AllianceRequests");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Status)
            .IsRequired();

        builder.Property(r => r.RequestedAtUtc)
            .IsRequired();

        builder.HasOne(r => r.RequestingParty)
            .WithMany()
            .HasForeignKey(r => r.RequestingPartyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ReceivingParty)
            .WithMany()
            .HasForeignKey(r => r.ReceivingPartyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}