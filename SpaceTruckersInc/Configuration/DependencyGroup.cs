using FluentValidation;

namespace SpaceTruckersInc.Configuration;

public static class DependencyGroup
{
    public static IServiceCollection AddDependencyGroup(this IServiceCollection services)
    {
        //Add Singleton Services

        // Add Scoped Services Register the action filter so it can be injected/resolved
        _ = services.AddScoped<FluentValidationActionFilter>();

        // Add Transient Services

        // FluentValidation registration
        _ = services.AddValidatorsFromAssembly(typeof(Program).Assembly);

        return services;
    }
}