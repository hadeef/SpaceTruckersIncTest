using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SpaceTruckersInc.Application.EventDispatchers;
using SpaceTruckersInc.Application.Services;
using SpaceTruckersInc.Application.Services.Interfaces;
using SpaceTruckersInc.Domain.Common.Interfaces;

namespace SpaceTruckersInc.Application.Configuration;

public static class DependencyGroup
{
    public static IServiceCollection AddApplicationDependencyGroup(this IServiceCollection services)
    {
        // AutoMapper
        _ = services.AddAutoMapper(cfg => { }, typeof(DependencyGroup).Assembly);

        //MediatR - Add Event Handlers
        _ = services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyGroup).Assembly));

        // Pipeline behaviors
        //_ = services.AddTransient(typeof(IValidationBehavior<,>), typeof(ValidationBehavior<,>));
        //_ = services.AddTransient(typeof(ILoggingBehavior<,>), typeof(LoggingBehavior<,>));

        // Register all Validators which implement IValidator from assembly
        _ = services.AddValidatorsFromAssembly(typeof(DependencyGroup).Assembly);

        // Services
        _ = services.AddScoped<ITripService, TripService>();
        _ = services.AddScoped<IDriverService, DriverService>();
        _ = services.AddScoped<IVehicleService, VehicleService>();
        _ = services.AddScoped<IRouteService, RouteService>();

        // Domain Event Dispatcher
        _ = services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        return services;
    }
}