# StudyNotesApi

REST API for study notes, built with ASP.NET Core, C#, EF Core, MySQL, Swagger, JWT, and XUnit in a layered architecture.

## Current status

We have completed the bootstrap step, the first domain modeling step, the EF Core/MySQL persistence setup, and the first cross-cutting API foundation items of the blueprint:

- the solution has been reorganized into `src/` and `tests/`
- the default `WeatherForecast` template code has been removed
- Swagger now documents the first HTTP endpoints
- the API root endpoint is available at `/api`
- the health endpoint is available at `/api/health` with JSON details
- the root `.env` file is loaded automatically during startup
- local MySQL points to `study-notes-db` in XAMPP
- the core domain entities have been modeled in English
- the unit test project now contains the first real tests for entity behavior
- EF Core infrastructure and entity mappings are now in place
- the initial EF Core migration has been generated and applied locally
- global exception handling is now centralized in middleware
- the project already follows controller-based routing and Swagger discovery
- test coverage validation is now available from a short root command
- the application contracts layer is now in place with service, repository, and security interfaces
- paging, sorting, and entity filter models are now ready for the service/repository implementations
- the infrastructure repository layer is now implemented against EF Core
- repository queries now support user scoping, filters, paging, and validated sorting
- JWT authentication is now configured in the API pipeline
- password hashing now uses `Argon2id`
- the Swagger UI is now prepared for Bearer token authentication

## Structure

```text
src/
  StudyNotesApi.Api/
  StudyNotesApi.Application/
  StudyNotesApi.Domain/
  StudyNotesApi.Infrastructure/
tests/
  StudyNotesApi.UnitTests/
```

## Configuration strategy

This repository loads a root `.env` file before `WebApplication.CreateBuilder(...)` runs. That means you should usually change local values in `.env`, not in `appsettings.json`.

Effective configuration precedence is:

1. operating system environment variables
2. values loaded from `.env`
3. fallback values from `appsettings.json` and `appsettings.Development.json`

This keeps local machine settings in one place while still preserving safe application defaults in `appsettings`.

### How `.env` actually works in this project

`.NET` does not load `.env` files by default.

In this repository, [EnvironmentFileLoader.cs](c:/VictorLocal/Projects/Personal/StudyNotesApi/src/StudyNotesApi.Api/Configurations/EnvironmentFileLoader.cs) reads the root `.env` file first and injects its values into the current process as environment variables.

After that, `WebApplication.CreateBuilder(...)` uses the normal ASP.NET Core configuration pipeline, which already knows how to read environment variables automatically.

So the flow is:

1. `EnvironmentFileLoader` reads `.env`
2. it calls `Environment.SetEnvironmentVariable(...)`
3. `CreateBuilder(...)` reads those values through the built-in environment variable provider
4. your application can access them through standard configuration keys such as `ConnectionStrings:DefaultConnection` and `Jwt:Secret`

That is why I removed the duplicated local values from `appsettings.json` and `appsettings.Development.json`: keeping the same local secrets in multiple files usually creates drift and confusion.

In short:

- `.env` is the local source of truth for machine-specific values
- `appsettings.json` keeps safe fallback defaults and non-secret configuration
- `EnvironmentFileLoader` is the bridge that makes `.env` participate in the normal .NET configuration system

## Engineering rules

This repository should evolve with:

- clean code as the default baseline
- `SRP` applied across controllers, services, repositories, and infrastructure helpers
- `DRY` without forcing premature abstractions
- thin controllers and business rules outside the HTTP layer
- Swagger, unit tests, coverage validation, and README updates delivered together with each increment

Cross-cutting items such as Swagger, error handling, tests, and coverage are not “final stage only” tasks here. They must keep evolving as new controllers, services, and domain behaviors are added.

### Local database

Expected local development database:

- Host: `localhost`
- Port: `3306`
- Database: `study-notes-db`
- User: `root`
- Password: empty string (common XAMPP default)

If you change your local MySQL password later, update only `.env`.

## Command guide

### General commands from the repository root

These commands work without extra parameters because the root folder contains [StudyNotesApi.sln](c:/VictorLocal/Projects/Personal/StudyNotesApi/StudyNotesApi.sln):

```powershell
dotnet restore
dotnet build
dotnet test
dotnet test -c Release
dotnet tool restore
```

Use the explicit form when you want to target something specific:

```powershell
dotnet tool restore
dotnet restore StudyNotesApi.sln
dotnet build StudyNotesApi.sln
dotnet test tests/StudyNotesApi.UnitTests/StudyNotesApi.UnitTests.csproj -c Release
```

On this local Windows environment, `dotnet test` in `Debug` can be blocked intermittently by Windows App Control for the generated test assembly. `Release` is the reliable mode here, so the README and the coverage script both use it on purpose.

### Running the API

`dotnet run` and `dotnet watch` need an executable project. The repository root is a solution folder, not a runnable project folder, so those commands cannot infer what to launch from the root by default.

Recommended standard workflow:

```powershell
cd src/StudyNotesApi.Api
dotnet run
dotnet watch run
```

`dotnet watch` may work too, but `dotnet watch run` is the clearest version for a web API project and is the one documented in this repository.

If you want to stay in the repository root, then you must target the API project explicitly:

```powershell
dotnet run --project src/StudyNotesApi.Api/StudyNotesApi.Api.csproj
dotnet watch --project src/StudyNotesApi.Api/StudyNotesApi.Api.csproj run
```

That is standard .NET CLI behavior and does not require any custom script.

### EF Core migrations

Recommended standard workflow:

```powershell
dotnet tool restore
cd src/StudyNotesApi.Api
```

Create a new migration:

```powershell
dotnet dotnet-ef migrations add MigrationName --project ..\StudyNotesApi.Infrastructure --output-dir Data\Migrations
```

Apply migrations to the local database:

```powershell
dotnet dotnet-ef database update --project ..\StudyNotesApi.Infrastructure
```

Remove the latest migration:

```powershell
dotnet dotnet-ef migrations remove --project ..\StudyNotesApi.Infrastructure
```

If you prefer to stay in the repository root, the explicit form is still valid:

```powershell
dotnet dotnet-ef migrations add MigrationName --project src/StudyNotesApi.Infrastructure --startup-project src/StudyNotesApi.Api --output-dir Data\Migrations
dotnet dotnet-ef database update --project src/StudyNotesApi.Infrastructure --startup-project src/StudyNotesApi.Api
```

The repository already includes the first migration in [src/StudyNotesApi.Infrastructure/Data/Migrations/20260411142404_InitialCreate.cs](c:/VictorLocal/Projects/Personal/StudyNotesApi/src/StudyNotesApi.Infrastructure/Data/Migrations/20260411142404_InitialCreate.cs).

## API endpoints available right now

### `GET /api`

Returns basic API metadata such as name, version, environment, and quick links.

### `GET /api/health`

Returns a JSON health report with:

- overall API health status
- current environment
- API version
- UTC timestamp
- individual checks for:
  - API online status
  - database connectivity

## Swagger

When the API is running, open:

```text
http://localhost:5080/swagger
```

Swagger is now configured with a Bearer security scheme. Once authentication endpoints exist, you will be able to paste a JWT token into Swagger UI and call protected endpoints directly.

## Health endpoint

When the API is running, you can probe:

```text
http://localhost:5080/api/health
```

The root API metadata endpoint is also available at:

```text
http://localhost:5080/api
```

Swagger should now show both endpoints because they are controller-based routes.

## Test coverage

You can run coverage validation from the repository root with:

```powershell
.\coverage.cmd
```

This wrapper:

- restores local .NET tools automatically
- runs the unit test suite in `Release`
- fails if the tests fail
- collects coverage with `dotnet-coverage`
- generates a Cobertura coverage report
- generates an HTML report in `TestResults/CoverageReport`
- prints overall line coverage for application files
- prints file-by-file coverage percentages for application files
- fails if any included file is below `100%`

Coverage validation intentionally excludes files that do not add value to test directly, such as:

- `Program.cs`
- DTO-only files
- migration files
- pure configuration and startup wiring files
- test files

If you want the explicit script form, this also works from the repository root:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\test-coverage.ps1
```

## Routing model

[Program.cs](c:/VictorLocal/Projects/Personal/StudyNotesApi/src/StudyNotesApi.Api/Program.cs) now stays intentionally small and delegates startup concerns to extension methods.

- `AddApiServices(...)` registers services, infrastructure, Swagger, health checks, and controllers
- `UseApiPipeline()` configures the middleware pipeline
- `MapApiEndpoints()` maps the root redirect and enables controller discovery through `app.MapControllers()`

New controller routes do not need to be manually listed one by one in `Program.cs`. Once a controller is registered and uses route attributes such as `[Route("api/[controller]")]`, ASP.NET Core discovers it automatically when `app.MapControllers()` runs.

## Error handling

Global error handling is centralized in [ExceptionHandlingMiddleware.cs](c:/VictorLocal/Projects/Personal/StudyNotesApi/src/StudyNotesApi.Api/Middlewares/ExceptionHandlingMiddleware.cs).

The middleware sits in the request pipeline before the controllers execute. If any controller, service, or dependency throws an exception that bubbles up, the middleware intercepts it and converts it into a standardized JSON error response with the correct HTTP status code.

Current mappings include:

- `ValidationException` -> `400`
- `UnauthorizedException` -> `401`
- `ForbiddenException` -> `403`
- `NotFoundException` -> `404`
- `ConflictException` -> `409`
- any other unhandled exception -> `500`

## Validation status

The current increment was validated with:

```powershell
dotnet build StudyNotesApi.sln
dotnet test tests/StudyNotesApi.UnitTests/StudyNotesApi.UnitTests.csproj -c Release
.\coverage.cmd
```

The solution now includes real unit tests for the domain, API foundation, application contract models, repository layer, and security components, a short coverage command, global error handling, controller-based Swagger discovery, JWT wiring, and the first infrastructure/migration setup.

## Next step

The next logical step is `Stage 6 - Auth`: registration, login, duplicate-email validation, password verification, token issuance, and the first public authentication endpoints.
