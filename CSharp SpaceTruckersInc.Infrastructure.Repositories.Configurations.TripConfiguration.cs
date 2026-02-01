using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpaceTruckersInc.Domain.Entities;

namespace SpaceTruckersInc.Infrastructure.Repositories.Configurations;

public sealed class TripConfiguration : IEntityTypeConfiguration<Trip>
{
    public void Configure(EntityTypeBuilder<Trip> builder)
    {
        builder.OwnsMany(p => p.TripEvents, tb =>
        {
            tb.WithOwner().HasForeignKey("TripId");
            tb.HasKey(e => e.Id);
            tb.Property(e => e.EventType).HasConversion<string>();
            tb.Property(e => e.Id).ValueGeneratedNever();
        });

    }
}