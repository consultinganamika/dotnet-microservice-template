# Architecture Overview

## Clean Architecture Layers

### 1. API Layer (Presentation)
- ASP.NET Core Controllers
- Request/Response handling
- HTTP middleware
- Error handling filters
- API versioning
- Swagger documentation

### 2. Application Layer (Business Logic)
- Use cases and business rules
- CQRS commands and queries
- MediatR handlers
- DTOs and mapping
- Validation logic
- Exception handling

### 3. Domain Layer (Core Business)
- Domain entities
- Value objects
- Domain events
- Business rules
- Constants and enums
- Pagination logic

### 4. Infrastructure Layer (Data Access)
- Database context (Entity Framework Core)
- Repositories
- External service integrations
- Messaging services
- Logging implementation

## CQRS Pattern Implementation

### Commands
- Represent actions that change state
- Handled by command handlers
- Return a result or void
- Example: CreateEmployeeCommand, UpdateEmployeeCommand

### Queries
- Represent data retrieval operations
- Read-only operations
- Handled by query handlers
- Return specific data
- Example: GetEmployeeByIdQuery, GetAllEmployeesQuery

## Dependency Flow

```
Presentation (API)
    ↓
Application (CQRS/MediatR)
    ↓
Domain (Entities, Business Rules)
    ↓
Infrastructure (Data Access, External Services)
```

## Design Patterns Used

1. **Repository Pattern** - Abstraction for data access
2. **Dependency Injection** - Loose coupling between layers
3. **Mediator Pattern** - CQRS implementation
4. **Middleware Pattern** - Request/response pipeline
5. **Factory Pattern** - Object creation
6. **Strategy Pattern** - Authentication strategies
7. **Decorator Pattern** - Logging and validation
