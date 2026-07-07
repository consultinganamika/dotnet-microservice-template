# .NET Core Microservice Template

A production-ready microservice template built with .NET Core 8/10, implementing CQRS pattern with MediatR, Clean Architecture, and industry best practices.

## Features

✅ **Clean Architecture** - 4-layer separation (API, Application, Domain, Infrastructure)
✅ **CQRS + MediatR** - Command Query Responsibility Segregation pattern
✅ **Authentication** - Company SSO + Admin/Support fallback options
✅ **Authorization** - Role-based access control (RBAC)
✅ **Global Exception Handling** - Centralized error management
✅ **Logging & Monitoring** - Serilog integration with health checks
✅ **API Versioning** - Multiple API version support
✅ **Swagger/OpenAPI** - Auto-generated API documentation
✅ **Fluent Validation** - Request validation with fluent API
✅ **Data Protection** - Sensitive data masking/unmasking
✅ **CORS Configuration** - Cross-origin request handling
✅ **Database Integration** - Entity Framework Core with repositories
✅ **External API Integration** - REST client services
✅ **Messaging Services** - Service Bus, Kafka, RabbitMQ support
✅ **Health Checks** - API health monitoring endpoints
✅ **.NET 8 & 10 Support** - Compatible with latest framework versions

## Quick Start

1. Clone the repository
2. Update connection strings in `appsettings.json`
3. Run migrations: `dotnet ef database update`
4. Start the API: `dotnet run`
5. Access Swagger: `https://localhost:5001/swagger`

## Documentation

- [Architecture Overview](docs/ARCHITECTURE.md)
- [Authentication & Authorization](docs/AUTHENTICATION.md)
- [API Versioning](docs/API_VERSIONING.md)
- [Logging and Monitoring](docs/LOGGING_AND_MONITORING.md)
- [Exception Handling](docs/EXCEPTION_HANDLING.md)
- [Deployment Guide](DEPLOYMENT.md)

## Project Structure

```
src/
├── Employee.API/                 # Presentation Layer
├── Employee.Application/         # Business Logic Layer
├── Employee.Domain/              # Domain Layer
└── Employee.Infrastructure/      # Data Access Layer
```

## Technologies

- .NET Core 8/10
- Entity Framework Core
- MediatR (CQRS)
- FluentValidation
- Serilog
- JWT Authentication
- Swagger/OpenAPI

## License

MIT License
