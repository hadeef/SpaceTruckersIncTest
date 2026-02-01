# SpaceTruckersInc

## Overview
Concise fleet & trip management API demonstrating Clean Architecture: `API` → `Application` → `Domain` → `Infrastructure`. Focused on domain rules, testability and simple local development using EF Core In‑Memory.

## Architecture & technology
- Clean layered structure with clear boundaries between projects.
- Persistence: EF Core with the `InMemory` provider for development and tests (configured in `ApplicationDbContext`). RowVersion concurrency tokens are applied only for non-InMemory databases.
- Mapping: `AutoMapper` (profiles in `Application/Mappings`).
- Validation: `FluentValidation` discovered via `services.AddValidatorsFromAssembly(...)` and enforced by `FluentValidationActionFilter` (registered in `Program.cs`).
- Domain events: dispatched using `IDomainEventDispatcher` and handled with `MediatR`.
- Caching: in-memory cache via `IMemoryCache` wrapped by `ICachingService`.
- Smart enums: `Ardalis.SmartEnum` for safe, named enum-like types (e.g., `VehicleModel`, `LicenseLevel`).

## Setup & build
Prerequisites:
- .NET 8 SDK
- (Optional) Visual Studio 2026

Visual Studio:
- Open solution and run __Build > Build Solution__.

## Run the application
Visual Studio:
- Set `SpaceTruckersInc` as the startup project and use __Debug > Start Debugging__ or __Debug > Start Without Debugging__.

Swagger UI is enabled in Development.

## Run unit tests
Tests are located in the `SpaceTruckersInc.UnitTest` project (MSTest).

## Assumptions
- Vehicle
  - Has `Model`, `CargoCapacity`, `Condition`, `Status`.
  - Damaged vehicles cannot be assigned to trips; marking damaged sets `Status` to `Maintenance`.
- Driver
  - Has `Name`, `LicenseLevel`, `Status`.
  - Only `Available` drivers can be assigned to trips.
- Route
  - `Origin`, `Destination`, `EstimatedDuration`, ordered `Checkpoints`. Duplicate/blank checkpoints ignored.
- Trip
  - Lifecycle: `Pending` → `InProgress` → `Completed` / `Cancelled`.
  - Timeline stored as owned `TripEvent` entities: checkpoints, incidents, delivery events, etc.

## Key design decisions
- GUID IDs (`Guid`): client-side generated identifiers simplify creation without DB round-trips, help distributed scenarios, and simplify testing and seeding.
- FluentValidation: validators auto-registered and applied through the MVC filter pipeline so controllers remain thin and validation runs consistently before action execution.
- EF Core InMemory for dev/tests: zero-config local development and CI. Note: InMemory differs from relational providers — concurrency semantics differ; row-versioning is enabled only for non-InMemory providers.
- SmartEnum (`Ardalis.SmartEnum`): safer parsing and name-based lookups vs plain enums.
- Domain events & repository: repositories persist entities and dispatch domain events after SaveChanges to decouple side effects from aggregate behavior.
- Owned types: `TripEvent` and `RouteCheckpoint` model aggregate internals and are mapped as owned collections for clarity.

## Next steps
- For production, swap the `InMemory` provider for a relational provider (SQL Server, PostgreSQL) and enable migrations.
- Add CI pipeline (build + tests) and integration tests targeting a real DB provider.

## Notes
- See `Program.cs`, `Application/Configuration/DependencyGroup.cs`, and `Infrastructure/Configuration/DependencyGroup.cs` for DI and registration details.