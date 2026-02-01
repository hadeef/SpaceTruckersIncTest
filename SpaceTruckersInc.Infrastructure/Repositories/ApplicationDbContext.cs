using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SmartEnum.EFCore;
using SpaceTruckersInc.Domain.Common;
using SpaceTruckersInc.Domain.Entities;
using SpaceTruckersInc.Domain.Enums;

namespace SpaceTruckersInc.Infrastructure.Repositories;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<Route> Routes => Set<Route>();
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureSmartEnum(); // Enable SmartEnum value converters globally

        // Ensure TripEvent isn't treated as a standalone entity before configuring as owned.
        _ = modelBuilder.Ignore<TripEvent>();

        _ = modelBuilder.Entity<Trip>(b =>
        {
            _ = b.OwnsMany(
                t => t.TripEvents,
                te =>
                {
                    _ = te.WithOwner().HasForeignKey("TripId");
                    _ = te.HasKey(nameof(TripEvent.Id));
                    _ = te.Property(e => e.OccurredOn).IsRequired();
                    _ = te.Property(e => e.Details).HasMaxLength(1000);
                    _ = te.Property(e => e.EventType)
                        .HasConversion(
                            v => v.Name,
                            v => TripEventType.FromName(v, true))
                        .IsRequired();
                    _ = te.ToTable("TripEvents");
                });

            // Ensure EF writes to the backing field _tripEvents
            _ = b.Navigation(t => t.TripEvents)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        _ = modelBuilder.Entity<Route>(b =>
        {
            _ = b.OwnsMany<RouteCheckpoint>("_checkpoints", cb =>
            {
                _ = cb.WithOwner().HasForeignKey("RouteId");
                _ = cb.Property<Guid>("Id");
                _ = cb.HasKey("Id");
                _ = cb.Property(c => c.Location).HasMaxLength(200).IsRequired();
                _ = cb.ToTable("RouteCheckpoints");
            });

            // ensure EF uses field access for the collection
            _ = b.Navigation("_checkpoints").UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        if (!Database.IsInMemory())
        {
            ApplyBaseEntityConfiguration(modelBuilder); // keeps RowVersion as concurrency token for real DBs
        }
        // else: skip RowVersion concurrency in InMemory to avoid false conflicts
    }

    private static void ApplyBaseEntityConfiguration(ModelBuilder modelBuilder)
    {
        foreach (IMutableEntityType? entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(t => typeof(Entity).IsAssignableFrom(t.ClrType) && !t.IsOwned()))
        {
            _ = modelBuilder
                .Entity(entityType.ClrType)
                .Property<byte[]>(nameof(Entity.RowVersion))
                .IsRowVersion();
        }
    }
}