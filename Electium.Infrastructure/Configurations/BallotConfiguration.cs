using System;
using System.Collections.Generic;
using System.Text;
using Electium.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Electium.Infrastructure.Configurations;

public class BallotConfiguration : IEntityTypeConfiguration<Ballot>
{
    public void Configure(EntityTypeBuilder<Ballot> builder)
    {
        builder.ToTable("Ballots");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.SubmittedAtUtc)
            .IsRequired();

        builder.HasOne(b => b.Election)
            .WithMany()
            .HasForeignKey(b => b.ElectionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}