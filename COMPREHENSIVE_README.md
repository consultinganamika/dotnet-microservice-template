# Employee Management System - .NET Microservice

## 📚 Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Project Structure](#project-structure)
4. [Technologies & Patterns](#technologies--patterns)
5. [Prerequisites](#prerequisites)
6. [Quick Start](#quick-start)
7. [Project Details](#project-details)
8. [API Endpoints](#api-endpoints)
9. [Inter-Microservice Communication](#inter-microservice-communication)
10. [Configuration](#configuration)
11. [Database Setup](#database-setup)
12. [Running the Application](#running-the-application)
13. [Testing](#testing)
14. [Troubleshooting](#troubleshooting)
15. [Additional Resources](#additional-resources)

---

## Overview

**Employee Management System** is an enterprise-grade .NET 8+ microservice that demonstrates best practices in modern cloud-native application development. It provides comprehensive employee management capabilities with event-driven architecture for seamless inter-microservice communication.

### Key Highlights

✅ **Clean Architecture** - Domain-driven design with clear layer separation  
✅ **CQRS Pattern** - Separate read and write operations using MediatR  
✅ **Event-Driven** - Async communication via Azure Service Bus  
✅ **Outbox Pattern** - Guaranteed event delivery with retry mechanism  
✅ **Domain Events** - Rich domain modeling with event sourcing concepts  
✅ **Validation** - FluentValidation for comprehensive input validation  
✅ **Security** - JWT authentication with role-based authorization  
✅ **Logging** - Structured logging with Serilog  
✅ **Caching** - In-memory and Redis caching support  
✅ **Health Checks** - Built-in health check endpoints  
✅ **API Documentation** - Swagger/OpenAPI auto-generated docs  
✅ **Docker Ready** - Includes Docker and Docker Compose files  

---

## Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     API Layer                               │
│  (Controllers, Middleware, Error Handling, Routing)         │
└────────────────┬────────────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────────────────┐
│              Application Layer (CQRS)                       │
│  (Commands, Queries, Handlers, DTOs, Validation, Events)   │
└────────────────┬────────────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────────────────┐
│               Domain Layer                                  │
│  (Entities, Value Objects, Domain Events, Aggregates)      │
└────────────────┬────────────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────────────────┐
│            Infrastructure Layer                             │
│  (Database, Repositories, Service Bus, Outbox, Logging)    │
└─────────────────────────────────────────────────────────────┘
```

### Inter-Microservice Communication

```
┌─────────────────────────────┐
│   Employee Service          │
│   (This Application)        │
└────────────┬────────────────┘
             │
             ├─ Publishes: EmployeeCreated
             ├─ Publishes: EmployeeUpdated
             ├─ Publishes: EmployeeDeleted
             ├─ Publishes: EmployeeSalaryUpdated
             │
             ▼
┌─────────────────────────────────────────────────────────┐
│        Azure Service Bus (Topic: employee-events)       │
└────────┬──────────────┬──────────────┬──────────────────┘
         │              │              │
    ┌────▼────┐  ┌─────▼─────┐ ┌────▼────┐
    │ Payment  │  │   Order   │ │    HR   │
    │ Service  │  │  Service  │ │Service  │
    └─────────┘  └───────────┘ └────────┘
```

---

## Project Structure

```
EmployeeManagementSystem/
├── EmployeeManagementSystem.sln          # Solution file
│
├── src/
│   ├── Employee.API/                     # API Layer (Presentation)
│   │   ├── Controllers/
│   │   │   ├── EmployeesController.cs    # Employee CRUD endpoints
│   │   │   └── BaseController.cs         # Base controller with common logic
│   │   ├── Middleware/
│   │   │   └── ErrorHandlingMiddleware.cs # Global error handling
│   │   ├── Extensions/                   # DI and configuration
│   │   │   ├── ApiVersioningExtensions.cs
│   │   │   ├── AuthenticationExtensions.cs
│   │   │   ├── AuthorizationExtensions.cs
│   │   │   ├── CachingExtensions.cs
│   │   │   ├── CorsExtensions.cs
│   │   │   ├── HealthCheckExtensions.cs
│   │   │   ├── MaskingExtensions.cs
│   │   │   ├── MessagingExtensions.cs    # Service Bus configuration
│   │   │   └── SwaggerExtensions.cs
│   │   ├── Program.cs                    # Application startup
│   │   ├── appsettings.json              # Configuration
│   │   ├── appsettings.Development.json  # Dev configuration
│   │   └── Employee.API.csproj           # Project file
│   │
│   ├── Employee.Application/             # Application Layer (CQRS)
│   │   ├── Common/
│   │   │   ├── Exceptions/               # Custom exceptions
│   │   │   ├── Models/                   # Pagination, responses
│   │   │   ├── Events/
│   │   │   │   └── IntegrationEvent.cs   # Integration events
│   │   │   ├── Messaging/
│   │   │   │   └── IEventPublisher.cs    # Event publishing interface
│   │   │   ├── InterMicroserviceEvents/
│   │   │   │   └── ExternalServiceEvents.cs  # Payment, Order, HR events
│   │   │   └── InterMicroserviceCommands/
│   │   │       └── ExternalServiceCommands.cs # Commands to other services
│   │   ├── Employees/
│   │   │   ├── Commands/
│   │   │   │   ├── CreateEmployee/
│   │   │   │   │   ├── CreateEmployeeCommand.cs
│   │   │   │   │   ├── CreateEmployeeCommandHandler.cs
│   │   │   │   │   └── CreateEmployeeCommandValidator.cs
│   │   │   │   ├── UpdateEmployee/
│   │   │   │   │   ├── UpdateEmployeeCommand.cs
│   │   │   │   │   ├── UpdateEmployeeCommandHandler.cs
│   │   │   │   │   └── UpdateEmployeeCommandValidator.cs
│   │   │   │   └── DeleteEmployee/
│   │   │   │       ├── DeleteEmployeeCommand.cs
│   │   │   │       └── DeleteEmployeeCommandHandler.cs
│   │   │   ├── Queries/
│   │   │   │   ├── GetAllEmployees/
│   │   │   │   │   ├── GetAllEmployeesQuery.cs
│   │   │   │   │   └── GetAllEmployeesQueryHandler.cs
│   │   │   │   └── GetEmployeeById/
│   │   │   │       ├── GetEmployeeByIdQuery.cs
│   │   │   │       └── GetEmployeeByIdQueryHandler.cs
│   │   │   ├── EventHandlers/
│   │   │   │   └── EmployeeDomainEventHandlers.cs
│   │   │   ├── Mappings/
│   │   │   │   └── EmployeeMappingProfile.cs # AutoMapper configuration
│   │   │   ├── EmployeeDto.cs             # Data Transfer Object
│   │   │   └── IEmployeeRepository.cs     # Repository interface
│   │   ├── InterMicroserviceHandlers/
│   │   │   └── ExternalServiceEventHandlers.cs # Event handlers from other services
│   │   ├── Extensions/
│   │   │   └── ServiceExtensions.cs       # DI configuration
│   │   └── Employee.Application.csproj
│   │
│   ├── Employee.Domain/                  # Domain Layer (Business Logic)
│   │   ├── Entities/
│   │   │   └── EmployeeEntity.cs         # Core employee domain entity
│   │   ├── Events/
│   │   │   └── DomainEvent.cs            # Domain events
│   │   └── Employee.Domain.csproj
│   │
│   └── Employee.Infrastructure/          # Infrastructure Layer (Persistence)
│       ├── Data/
│       │   ├── EmployeeDbContext.cs      # EF Core DbContext
│       │   └── Repositories/
│       │       ├── EmployeeRepository.cs # Employee repository implementation
│       │       └── OutboxRepository.cs   # Outbox repository implementation
│       ├── Extensions/
│       │   ├── ServiceExtensions.cs      # DI configuration
│       │   └── ServiceBusExtensions.cs   # Service Bus configuration
│       ├── Messaging/
│       │   ├── ServiceBus/
│       │   │   ├── ServiceBusEventPublisher.cs    # Publishes to Service Bus
│       │   │   └── ServiceBusEventSubscriber.cs   # Listens to Service Bus
│       │   ├── Outbox/
│       │   │   └── OutboxMessage.cs
│       │   ├── IOutboxRepository.cs
│       │   ├── OutboxEventPublisher.cs   # Outbox pattern implementation
│       │   └── OutboxPublisherBackgroundService.cs # Retry background service
│       └── Employee.Infrastructure.csproj
│
├── docs/
│   ├── README.md                         # Main documentation (this file)
│   └── ARCHITECTURE.md                   # Detailed architecture guide
│
├── Dockerfile                            # Docker container configuration
├── docker-compose.yml                    # Local development environment
├── .gitignore                            # Git ignore file
├── README.md                             # Project documentation
└── SERVICE_BUS_SETUP.md                  # Azure Service Bus setup guide
```

---

## Technologies & Patterns

### Core Technologies

| Component | Technology | Version |
|-----------|-----------|----------|
| Framework | .NET | 8.0+ or 10.0 |
| Language | C# | Latest |
| Database | SQL Server | 2019+ |
| ORM | Entity Framework Core | 8.0 |
| Message Bus | Azure Service Bus | Latest |
| Caching | Redis/In-Memory | Latest |
| Web API | ASP.NET Core | 8.0+ |
| API Docs | Swagger/OpenAPI | 3.0 |

### Design Patterns Implemented

| Pattern | Purpose | Implementation |
|---------|---------|----------------|
| **Clean Architecture** | Separation of concerns | 4-layer architecture |
| **CQRS** | Separate read/write | Commands, Queries, Handlers |
| **Domain Events** | Rich domain modeling | DomainEvent base class |
| **Integration Events** | Inter-service communication | IntegrationEvent, Service Bus |
| **Outbox Pattern** | Guaranteed delivery | OutboxMessage, Background service |
| **Repository Pattern** | Data abstraction | IEmployeeRepository |
| **Dependency Injection** | Loose coupling | Microsoft.Extensions.DependencyInjection |
| **MediatR** | Command/Query dispatch | Request handlers |
| **AutoMapper** | Object mapping | EmployeeMappingProfile |
| **FluentValidation** | Input validation | Command validators |
| **Specification Pattern** | Complex queries | (Ready to implement) |

### Architectural Principles

✅ **Single Responsibility Principle (SRP)**  
✅ **Open/Closed Principle (OCP)**  
✅ **Liskov Substitution Principle (LSP)**  
✅ **Interface Segregation Principle (ISP)**  
✅ **Dependency Inversion Principle (DIP)**  

---

## Prerequisites

### Required

- **Visual Studio 2022** or **Visual Studio Code**
- **.NET 8 SDK** or later (download from [dotnet.microsoft.com](https://dotnet.microsoft.com))
- **SQL Server 2019** or later
  - Alternative: Use Docker (included)
- **Git** for version control

### Optional

- **Docker Desktop** for containerization
- **Azure CLI** for Azure Service Bus setup
- **Postman** or **Thunder Client** for API testing
- **SQL Server Management Studio (SSMS)** for database management

### Check Prerequisites

```bash
# Check .NET installation
dotnet --version

# Check Git installation
git --version

# Check Docker installation
docker --version
```

---

## Quick Start

### 1. Clone the Repository

```bash
git clone https://github.com/consultinganamika/dotnet-microservice-template.git
cd dotnet-microservice-template
```

### 2. Restore NuGet Packages

```bash
dotnet restore
```

### 3. Setup Database

**Option A: Using Docker (Recommended)**

```bash
docker-compose up -d
```

This starts SQL Server on `localhost:1433` with default credentials:
- **Server**: localhost
- **Username**: sa
- **Password**: YourPassword123!

**Option B: Using Local SQL Server**

Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=EmployeeDb;Integrated Security=true;Encrypt=false;TrustServerCertificate=true;"
  }
}
```

### 4. Run Database Migrations

```bash
# Navigate to Infrastructure project
cd src/Employee.Infrastructure

# Create and apply migration
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 5. Run the Application

```bash
# From root directory
cd src/Employee.API
dotnet run
```

The API will be available at:
- **HTTP**: http://localhost:5000
- **HTTPS**: https://localhost:5001
- **Swagger UI**: https://localhost:5001/swagger

---

## Project Details

### Employee.Domain

**Purpose**: Core business logic and domain entities  
**Key Components**:
- `EmployeeEntity` - Main aggregate root
- `DomainEvent` - Base class for domain events
- `EmployeeCreatedDomainEvent` - Event when employee created
- `EmployeeUpdatedDomainEvent` - Event when employee updated
- `EmployeeDeletedDomainEvent` - Event when employee deleted

**Dependencies**: None (pure domain logic)

### Employee.Application

**Purpose**: Application logic, CQRS, validation, and orchestration  
**Key Components**:
- **Commands**: CreateEmployeeCommand, UpdateEmployeeCommand, DeleteEmployeeCommand
- **Queries**: GetAllEmployeesQuery, GetEmployeeByIdQuery
- **Handlers**: MediatR request handlers
- **Validators**: FluentValidation rules
- **DTOs**: EmployeeDto for data transfer
- **Events**: Integration events for inter-service communication

**Dependencies**:
- MediatR (CQRS dispatch)
- FluentValidation (Input validation)
- AutoMapper (Object mapping)

### Employee.Infrastructure

**Purpose**: Data access, external services, and infrastructure concerns  
**Key Components**:
- `EmployeeDbContext` - EF Core DbContext
- `EmployeeRepository` - Data access for employees
- `OutboxRepository` - Data access for outbox messages
- `ServiceBusEventPublisher` - Azure Service Bus integration
- `ServiceBusEventSubscriber` - Event subscription
- `OutboxEventPublisher` - Outbox pattern implementation
- `OutboxPublisherBackgroundService` - Retry mechanism

**Dependencies**:
- Entity Framework Core
- Azure Service Bus
- Serilog

### Employee.API

**Purpose**: HTTP endpoints, middleware, and request handling  
**Key Components**:
- `EmployeesController` - REST endpoints
- `ErrorHandlingMiddleware` - Global exception handling
- `Program.cs` - Application configuration
- Extension methods for DI and middleware setup

**Key Features**:
- API versioning support
- JWT authentication
- Role-based authorization
- CORS configuration
- Health checks
- Structured logging

---

## API Endpoints

### Employee Operations

#### Get All Employees (Paginated)

```http
GET /api/v1/employees?pageNumber=1&pageSize=10&searchTerm=john
```

**Response**: 200 OK
```json
{
  "data": [
    {
      "id": 1,
      "firstName": "John",
      "lastName": "Doe",
      "email": "john@example.com",
      "phoneNumber": "+1234567890",
      "department": "Engineering",
      "position": "Senior Developer",
      "salary": 150000,
      "hireDate": "2020-01-15",
      "isActive": true
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 50
}
```

#### Get Employee by ID

```http
GET /api/v1/employees/{id}
```

**Response**: 200 OK
```json
{
  "id": 1,
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com",
  "phoneNumber": "+1234567890",
  "department": "Engineering",
  "position": "Senior Developer",
  "salary": 150000,
  "hireDate": "2020-01-15",
  "isActive": true
}
```

#### Create Employee

```http
POST /api/v1/employees
Content-Type: application/json
Authorization: Bearer <token>

{
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane@example.com",
  "phoneNumber": "+1987654321",
  "department": "Marketing",
  "position": "Product Manager",
  "salary": 120000,
  "dateOfBirth": "1990-05-20",
  "hireDate": "2023-01-01"
}
```

**Response**: 201 Created

#### Update Employee

```http
PUT /api/v1/employees/{id}
Content-Type: application/json
Authorization: Bearer <token>

{
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane@example.com",
  "phoneNumber": "+1987654321",
  "department": "Marketing",
  "position": "Senior Product Manager",
  "salary": 140000,
  "isActive": true
}
```

**Response**: 200 OK

#### Delete Employee

```http
DELETE /api/v1/employees/{id}
Authorization: Bearer <token>
```

**Response**: 204 No Content

#### Health Check

```http
GET /health
```

**Response**: 200 OK
```json
{
  "status": "Healthy"
}
```

---

## Inter-Microservice Communication

### Events Published by This Service

1. **EmployeeCreatedIntegrationEvent**
   - Published when: New employee created
   - Consumed by: Payment, Order, HR, Compliance services
   - Contains: Employee ID, name, email, department, salary, hire date

2. **EmployeeUpdatedIntegrationEvent**
   - Published when: Employee information updated
   - Consumed by: Payment, Order, HR, Compliance services
   - Contains: Updated employee details

3. **EmployeeDeletedIntegrationEvent**
   - Published when: Employee deleted
   - Consumed by: Payment, Order, HR, Compliance services
   - Contains: Employee ID, email, deletion date

4. **EmployeeSalaryUpdatedIntegrationEvent**
   - Published when: Employee salary changed
   - Consumed by: Payment, Compliance services
   - Contains: Old salary, new salary, effective date

### Events Consumed from Other Services

#### Payment Service Events
- `PaymentProcessedEvent` - Payment completed for employee
- `PaymentFailedEvent` - Payment failed
- `SalaryAdjustmentEvent` - Salary adjusted

#### Order Service Events
- `OrderCreatedEvent` - Employee order created
- `OrderShippedEvent` - Order shipped
- `OrderCancelledEvent` - Order cancelled

#### HR Service Events
- `AttendanceRecordedEvent` - Attendance recorded
- `LeaveRequestApprovedEvent` - Leave request approved

#### Compliance Service Events
- `AuditLogCreatedEvent` - Audit log entry created
- `ComplianceViolationDetectedEvent` - Compliance issue detected

### Guaranteed Delivery (Outbox Pattern)

The application ensures reliable event delivery using the Outbox Pattern:

```
Step 1: Employee operation → Save employee + Create outbox message (same transaction)
Step 2: Publish event to Azure Service Bus
Step 3: Mark outbox message as processed
Step 4: If Step 2-3 fails → Background service retries every 30 seconds
Step 5: Maximum 5 retries before logging failure
```

---

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EmployeeDb;Integrated Security=true;Encrypt=false;TrustServerCertificate=true;",
    "ServiceBus": "Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=your-key"
  },
  "JwtSettings": {
    "Secret": "your-super-secret-key-min-32-chars",
    "Issuer": "employee-api",
    "Audience": "employee-api-audience",
    "ExpirationMinutes": 60
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000", "https://localhost:3000"]
  },
  "ServiceBusSettings": {
    "TopicName": "employee-events",
    "SubscriptionName": "employee-service-subscription"
  }
}
```

### User Secrets (Recommended for sensitive data)

```bash
# Initialize user secrets
dotnet user-secrets init

# Set secrets
dotnet user-secrets set "JwtSettings:Secret" "your-secret-key"
dotnet user-secrets set "ConnectionStrings:ServiceBus" "your-connection-string"
```

---

## Database Setup

### Create Initial Database

```bash
cd src/Employee.Infrastructure

# Add migration
dotnet ef migrations add InitialCreate

# Apply migration
dotnet ef database update
```

### View Database

```bash
# Using SQL Server Management Studio
# Server: localhost (or docker-compose service)
# Authentication: SQL Server Authentication
# Login: sa
# Password: YourPassword123!

# Or using CLI
sqlcmd -S localhost -U sa -P YourPassword123! -Q "SELECT * FROM Employees"
```

### Reset Database

```bash
cd src/Employee.Infrastructure

# Remove latest migration
dotnet ef migrations remove

# Delete database
dotnet ef database drop

# Reapply migrations
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

## Running the Application

### Local Development

```bash
# Terminal 1: Start Docker containers
docker-compose up -d

# Terminal 2: Navigate to API project
cd src/Employee.API

# Run the application
dotnet run

# Or with watch mode (auto-restart on changes)
dotnet watch run
```

### Visual Studio

1. Open `EmployeeManagementSystem.sln` in Visual Studio 2022
2. Set `Employee.API` as startup project (right-click → Set as Startup Project)
3. Press F5 or click "Run"
4. Swagger UI opens automatically at https://localhost:5001/swagger

### Docker

```bash
# Build image
docker build -t employee-api:latest .

# Run container
docker run -p 5000:5000 -p 5001:5001 \
  -e ConnectionStrings__DefaultConnection="Server=host.docker.internal;Database=EmployeeDb;..." \
  employee-api:latest
```

### Production Build

```bash
# Build in Release mode
dotnet build -c Release

# Publish
dotnet publish -c Release -o ./publish
```

---

## Testing

### Using Swagger UI

1. Navigate to https://localhost:5001/swagger
2. All endpoints are documented with request/response examples
3. Click "Try it out" to test endpoints
4. Authentication: Copy JWT token to "Authorize" button

### Using Postman

1. Import collection from Swagger: `/swagger/v1/swagger.json`
2. Set up environment variables:
   - `base_url`: http://localhost:5000
   - `token`: Your JWT token
3. Test endpoints

### Using cURL

```bash
# Get all employees
curl -X GET https://localhost:5001/api/v1/employees

# Create employee
curl -X POST https://localhost:5001/api/v1/employees \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com",
    "phoneNumber": "+1234567890",
    "department": "Engineering",
    "position": "Developer",
    "salary": 100000,
    "dateOfBirth": "1990-01-01",
    "hireDate": "2023-01-01"
  }'
```

### Unit Testing Structure (Ready to implement)

```
Employee.Tests/
├── Unit/
│   ├── Application/
│   ├── Domain/
│   └── Infrastructure/
├── Integration/
│   ├── API/
│   └── Database/
└── Employee.Tests.csproj
```

---

## Troubleshooting

### Issue: "Connection refused" to SQL Server

**Solution**:
```bash
# Check Docker container
docker-compose ps

# Restart containers
docker-compose restart

# Check connection string
# Ensure Server=localhost (not 127.0.0.1 for Integrated Security)
```

### Issue: EF Core migrations not found

**Solution**:
```bash
# Ensure you're in Infrastructure project directory
cd src/Employee.Infrastructure

# Remove and recreate migrations
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Issue: "Project already exists" when adding projects to solution

**Solution**:
```bash
# Use absolute paths
dotnet sln add src/Employee.API/Employee.API.csproj
```

### Issue: Service Bus connection fails

**Solution**:
```bash
# Check connection string format
# Should be: Endpoint=sb://namespace.servicebus.windows.net/;SharedAccessKeyName=...;SharedAccessKey=...

# Verify namespace exists in Azure
az servicebus namespace list

# Create if needed
az servicebus namespace create --name your-namespace --resource-group your-rg
```

### Issue: Port already in use

**Solution**:
```bash
# Find process using port 5001
lsof -i :5001  # macOS/Linux
netstat -ano | findstr :5001  # Windows

# Kill process or use different port
dotnet run --urls "https://localhost:5002"
```

---

## Additional Resources

### Documentation
- [Architecture Guide](./ARCHITECTURE.md)
- [Azure Service Bus Setup](./SERVICE_BUS_SETUP.md)
- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [MediatR Documentation](https://github.com/jbogard/MediatR)

### Tools & Libraries
- [.NET CLI](https://docs.microsoft.com/en-us/dotnet/core/tools/)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/)
- [Docker](https://www.docker.com/)
- [Azure Service Bus](https://azure.microsoft.com/en-us/services/service-bus/)
- [Swagger/OpenAPI](https://swagger.io/)

### Learning Resources
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [Event Sourcing](https://martinfowler.com/eaaDev/EventSourcing.html)
- [Microservices](https://martinfowler.com/articles/microservices.html)

---

## Support

For issues or questions:
1. Check [Troubleshooting](#troubleshooting) section
2. Review documentation files
3. Check application logs in `logs/` directory
4. Contact: support@company.com

---

## License

MIT License - See LICENSE file for details

---

**Last Updated**: July 2026  
**Version**: 1.0.0  
**Status**: Production Ready ✅
