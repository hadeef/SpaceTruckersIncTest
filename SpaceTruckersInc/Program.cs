using SpaceTruckersInc.Application.Configuration;
using SpaceTruckersInc.Configuration;
using SpaceTruckersInc.Infrastructure.Configuration;
using SpaceTruckersInc.Middlewares;

namespace SpaceTruckersInc;

public static class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        _ = builder.Services.AddLogging(loggingBuilder =>
        {
            _ = loggingBuilder.AddConsole();
            _ = loggingBuilder.AddDebug();
        });

        // Add services to the container.

        _ = builder.Services.AddMemoryCache();

        //All Dependency Injection is registered in the DependencyGroup class
        _ = builder.Services.AddDependencyGroup();
        _ = builder.Services.AddInfrastructureDependencyGroup();
        _ = builder.Services.AddApplicationDependencyGroup();

        // Add controllers and register the filter globally so it runs before actions
        _ = builder.Services.AddControllers(options =>
        {
            _ = options.Filters.Add<FluentValidationActionFilter>();
        });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        _ = builder.Services.AddEndpointsApiExplorer();
        _ = builder.Services.AddSwaggerGen();

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            _ = app.UseSwagger();
            _ = app.UseSwaggerUI();
        }

        // global exception handling middleware
        _ = app.UseMiddleware<GlobalExceptionHandler>();

        _ = app.UseHttpsRedirection();

        _ = app.UseAuthorization();

        _ = app.MapControllers();

        app.Run();
    }
}