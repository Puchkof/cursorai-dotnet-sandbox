# HeroBoxAI

A .NET 8 Web API built following Clean Architecture principles.

## Project Structure

The solution follows Clean Architecture principles, organized into the following projects:

### Source Code (src)

- **HeroBoxAI.Domain**: Contains all entities, enums, exceptions, interfaces, types and logic specific to the domain layer.
  - `Entities`: Domain entities
  - `Enums`: Enumeration types
  - `Events`: Domain events
  - `Exceptions`: Custom domain exceptions
  - `ValueObjects`: Value objects used by entities

- **HeroBoxAI.Application**: Contains all application logic and use cases.
  - `Common`: Cross-cutting concerns
    - `Behaviors`: Pipeline behaviors (validation, logging, etc.)
    - `Interfaces`: Application interfaces
    - `Models`: DTOs and other models
  - `Features`: Application features organized by business capabilities

- **HeroBoxAI.Infrastructure**: Contains classes for accessing external resources such as databases, file systems, web services, etc.
  - `Data`: Database-related components
    - `Configurations`: Entity Framework configurations
    - `Migrations`: Database migrations
    - `Repositories`: Repository implementations
  - `Services`: Implementation of application services

- **HeroBoxAI.WebApi**: Contains the API controllers, middleware configurations, and other web-related components.
  - `Controllers`: API controllers
  - `Filters`: Action filters
  - `Middleware`: Custom middleware components

### Tests (tests)

- **HeroBoxAI.Domain.Tests**: Unit tests for the Domain layer
- **HeroBoxAI.Application.Tests**: Unit tests for the Application layer
- **HeroBoxAI.Infrastructure.Tests**: Integration tests for the Infrastructure layer
- **HeroBoxAI.WebApi.Tests**: Integration tests for the Web API

## Key Technologies

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- MediatR (CQRS pattern)
- xUnit (Testing)

## Getting Started

### Prerequisites

- .NET 8 SDK
- Your favorite IDE (Visual Studio, Rider, VS Code)

### Running the Application

```bash
# Navigate to the WebApi project
cd src/HeroBoxAI.WebApi

# Run the application
dotnet run
```

### Running Tests

```bash
# Run all tests
dotnet test
``` 