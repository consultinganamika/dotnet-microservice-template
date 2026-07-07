# Employee Management Microservice

A comprehensive .NET 8+ microservice template for employee management with enterprise-level patterns and best practices.

## Features

- **Clean Architecture**: Separation of concerns with Domain, Application, Infrastructure, and API layers
- **CQRS Pattern**: Command Query Responsibility Segregation using MediatR
- **API Versioning**: Support for multiple API versions
- **Authentication & Authorization**: JWT-based authentication with role-based policies
- **Logging**: Structured logging with Serilog
- **Error Handling**: Comprehensive global error handling middleware
- **Validation**: FluentValidation for input validation
- **Caching**: Support for both in-memory and Redis caching
- **Health Checks**: Built-in health check endpoints
- **CORS**: Configurable CORS policy
- **Data Protection**: Sensitive data masking
- **Entity Framework Core**: Database access layer with SQL Server support
- **Swagger/OpenAPI**: Auto-generated API documentation

## Project Structure

```
src/
├── Employee.API/              # API Layer (Controllers, Middleware, Extensions)
├── Employee.Application/      # Application Layer (Commands, Queries, Handlers)
├── Employee.Domain/           # Domain Layer (Entities)
└── Employee.Infrastructure/   # Infrastructure Layer (DbContext, Repositories)
```

## Prerequisites

- .NET 8 or .NET 10
- SQL Server 2019 or later
- Visual Studio 2022 or Visual Studio Code

## Configuration

### appsettings.json

Update the configuration file with your settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EmployeeDb;Integrated Security=true;"
  },
  "JwtSettings": {
    "Secret": "your-secret-key",
    "Issuer": "employee-api",
    "Audience": "employee-api-audience",
    "ExpirationMinutes": 60
  }
}
```

## Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd dotnet-microservice-template
```

### 2. Restore NuGet Packages

```bash
dotnet restore
```

### 3. Create Database

```bash
cd src/Employee.Infrastructure
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4. Run the Application

```bash
cd src/Employee.API
dotnet run
```

The API will be available at `https://localhost:5001`

## API Endpoints

### Employee Management

- `GET /api/v1/employees` - Get all employees (paginated)
- `GET /api/v1/employees/{id}` - Get employee by ID
- `POST /api/v1/employees` - Create new employee (requires Manager role)
- `PUT /api/v1/employees/{id}` - Update employee (requires Manager role)
- `DELETE /api/v1/employees/{id}` - Delete employee (requires Admin role)

### Health Check

- `GET /health` - Health check endpoint

## Authentication

The API uses JWT tokens for authentication. Include the token in the Authorization header:

```
Authorization: Bearer <your-jwt-token>
```

## Authorization Policies

- `AdminOnly` - Requires Admin role
- `ManagerOrAbove` - Requires Manager or Admin role
- `SupportTeam` - Requires Support or Admin role
- `EmployeeOrAbove` - Requires Employee, Manager, or Admin role

## Database Schema

### Employees Table

- Id (int, Primary Key)
- FirstName (nvarchar(100))
- LastName (nvarchar(100))
- Email (nvarchar(255), Unique)
- PhoneNumber (nvarchar(20))
- Department (nvarchar(100))
- Position (nvarchar(100))
- Salary (decimal(18,2))
- DateOfBirth (datetime2)
- HireDate (datetime2)
- IsActive (bit)
- CreatedBy (nvarchar(255))
- CreatedAt (datetime2)
- ModifiedBy (nvarchar(255), nullable)
- ModifiedAt (datetime2, nullable)
- DeletedBy (nvarchar(255), nullable)
- DeletedAt (datetime2, nullable)
- IsDeleted (bit)

## Logging

The application uses Serilog for structured logging. Logs are written to:

- Console output
- File: `logs/app-*.txt` (rolling daily)

## Testing

Swagger UI is available at `https://localhost:5001/swagger` for testing API endpoints.

## Best Practices Implemented

1. **Separation of Concerns**: Each layer has a specific responsibility
2. **Dependency Injection**: All services are registered in DI container
3. **Async/Await**: Async operations throughout
4. **Error Handling**: Global exception handling middleware
5. **Validation**: Input validation using FluentValidation
6. **Logging**: Comprehensive logging for debugging
7. **Security**: JWT authentication, role-based authorization
8. **Scalability**: Support for caching and multiple API versions

## Contributing

1. Create a feature branch
2. Make your changes
3. Submit a pull request

## License

MIT

## Support

For support, email support@company.com
