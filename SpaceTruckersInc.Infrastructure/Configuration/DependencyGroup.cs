using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SpaceTruckersInc.Application.Common.Interfaces;
using SpaceTruckersInc.Domain.Interfaces;
using SpaceTruckersInc.Infrastructure.Repositories;
using SpaceTruckersInc.Infrastructure.Services;

namespace SpaceTruckersInc.Infrastructure.Configuration;

public static class DependencyGroup
{
    public static IServiceCollection AddInfrastructureDependencyGroup(this IServiceCollection services)
    {
        //Add DbContext
        _ = services.AddDbContext<ApplicationDbContext>(options =>
        {
            _ = options.UseInMemoryDatabase("DbInMemory");
        });

        //Add Singleton Services
        _ = services.AddSingleton<ICachingService, CachingService>();

        // Add Scoped Services
        _ = services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        _ = services.AddScoped<ITripRepository, TripRepository>();
        _ = services.AddScoped<IDriverRepository, DriverRepository>();
        _ = services.AddScoped<IVehicleRepository, VehicleRepository>();
        _ = services.AddScoped<IRouteRepository, RouteRepository>();

        // Add Transient Services

        return services;
    }
}