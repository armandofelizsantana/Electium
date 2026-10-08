using System;
using System.Collections.Generic;
using System.Text;
using Electium.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Electium.Infrastructure.Configurations;

public class ElectivePositionConfiguration
    : IEntityTypeConfiguration<ElectivePosition>
{
    public void Configure(EntityTypeBuilder<ElectivePosition> builder)
    {
        builder.ToTable("ElectivePositions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(500);
    }
}