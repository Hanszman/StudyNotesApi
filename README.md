# StudyNotesApi

REST API for study notes, built with ASP.NET Core, C#, EF Core, MySQL, Swagger, JWT, and XUnit in a layered architecture.

## Current status

We have completed the bootstrap step of the blueprint:

- the solution has been reorganized into `src/` and `tests/`
- the default `WeatherForecast` template code has been removed
- Swagger is configured at a basic level
- the root `.env` file is loaded automatically during startup
- local MySQL points to `study-notes-db` in XAMPP
- the test project is scaffolded and ready for the first real unit tests in the next step

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
```

Use the explicit form when you want to target something specific:

```powershell
dotnet restore StudyNotesApi.sln
dotnet build StudyNotesApi.sln
dotnet test tests/StudyNotesApi.UnitTests/StudyNotesApi.UnitTests.csproj
```

### Running the API

`dotnet run` and `dotnet watch` need an executable project. The repository root is a solution folder, not a runnable project folder, so those commands cannot infer what to launch from the root by default.

Recommended standard workflow:

```powershell
cd src/StudyNotesApi.Api
dotnet run
dotnet watch
```

`dotnet watch run` is also valid there if you prefer to be explicit.

If you want to stay in the repository root, then you must target the API project explicitly:

```powershell
dotnet run --project src/StudyNotesApi.Api/StudyNotesApi.Api.csproj
dotnet watch --project src/StudyNotesApi.Api/StudyNotesApi.Api.csproj run
```

That is standard .NET CLI behavior and does not require any custom script.

## Swagger

When the API is running, open:

```text
http://localhost:5080/swagger
```

## Validation status

The bootstrap was validated with:

```powershell
dotnet build StudyNotesApi.sln
dotnet test tests/StudyNotesApi.UnitTests/StudyNotesApi.UnitTests.csproj
```

At this stage the test project is present and executes correctly, but it still has no real tests yet. The first business tests will be added together with the first domain/application increments.

## Next step

The next logical step is `Stage 1 - Domain`: model `User`, `Category`, `Tag`, `Note`, and `NoteTag`, then follow up with the first unit tests as the domain and services start to gain behavior.
