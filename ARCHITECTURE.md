# Architecture Deep Dive

This document provides an in-depth explanation of the system architecture, design decisions, and how all components work together.

## Table of Contents

1. [Layered Architecture](#layered-architecture)
2. [CQRS Pattern](#cqrs-pattern)
3. [Domain Events](#domain-events)
4. [Integration Events](#integration-events)
5. [Outbox Pattern](#outbox-pattern)
6. [Data Flow](#data-flow)
7. [Dependency Graph](#dependency-graph)
8. [Database Schema](#database-schema)

## Layered Architecture

### Layer 1: Domain Layer (Employee.Domain)

**Responsibility**: Pure business logic and domain entities

**Components**:
- `EmployeeEntity` - The core aggregate root
- `DomainEvent` - Base event class
- Domain-specific exceptions (if any)
- Business rules and validations

**Characteristics**:
- No external dependencies
- Completely testable
- Contains business rules
- Implements ubiquitous language

**Example**:
```csharp
public class EmployeeEntity
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    // Business logic
    public string GetFullName() => $"{FirstName} {LastName}";
    
    // Validation
    public bool IsValidForHiring() => /* business rules */
}
```

### Layer 2: Application Layer (Employee.Application)

**Responsibility**: Application logic, CQRS orchestration, and use cases

**Components**:
- Commands (write operations)
- Queries (read operations)
- Command/Query Handlers
- DTOs (Data Transfer Objects)
- Validators
- Exceptions
- Mappings
- Events

**Characteristics**:
- Depends only on Domain layer
- No database or external service details
- Orchestrates domain logic
- Handles use cases

**Example Flow**:
```
CreateEmployeeCommand
    ↓
CreateEmployeeCommandValidator (validates)
    ↓
CreateEmployeeCommandHandler (executes)
    ├─ Validate email uniqueness (via repository)
    ├─ Create domain event
    ├─ Persist via repository
    ├─ Publish domain event
    └─ Return DTO
```

### Layer 3: Infrastructure Layer (Employee.Infrastructure)

**Responsibility**: External services, persistence, and infrastructure concerns

**Components**:
- DbContext and Entity Mappings
- Repository implementations
- Service Bus publishers/subscribers
- Outbox pattern implementation
- Background services
- Logging setup

**Characteristics**:
- Implements interfaces from Application layer
- Handles database operations
- Manages external services
- Deals with technical concerns

**Example**:
```csharp
public class EmployeeRepository : IEmployeeRepository
{
    private readonly EmployeeDbContext _context;
    
    public async Task<EmployeeEntity> AddAsync(EmployeeEntity employee)
    {
        // Technical implementation
        await _context.Employees.AddAsync(employee);
        await _context.SaveChangesAsync();
        return employee;
    }
}
```

### Layer 4: Presentation Layer (Employee.API)

**Responsibility**: HTTP endpoints, request handling, and external communication

**Components**:
- Controllers (REST endpoints)
- Middleware
- Extension methods for DI
- Error handling
- Request/Response mapping

**Characteristics**:
- Entry point for external requests
- Converts HTTP to application commands/queries
- Handles authentication/authorization
- Formats responses

**Example**:
```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class EmployeesController : BaseController
{
    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> CreateEmployee(
        [FromBody] CreateEmployeeCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetEmployee), 
            new { id = result.Id }, result);
    }
}
```

## CQRS Pattern

### Command Side (Write Operations)

**Flow**:
```
Client Request
    ↓
Controller (receives CreateEmployeeCommand)
    ↓
MediatR Pipeline
    ├─ Validators execute
    └─ Handler executes
    ↓
CreateEmployeeCommandHandler
    ├─ Load dependencies
    ├─ Execute business logic
    ├─ Persist to database
    ├─ Publish domain event
    └─ Return result
    ↓
Controller (formats response)
    ↓
HTTP 201 Created
```

**Key Benefits**:
- Clear separation of read and write concerns
- Different optimization strategies for reads vs writes
- Simpler testing (mock either read or write path)
- Easier scaling (read replicas for queries)

### Query Side (Read Operations)

**Flow**:
```
Client Request
    ↓
Controller (receives GetEmployeeByIdQuery)
    ↓
MediatR Pipeline
    └─ Handler executes
    ↓
GetEmployeeByIdQueryHandler
    ├─ Query database
    ├─ Apply caching if available
    └─ Return DTO
    ↓
Controller (formats response)
    ↓
HTTP 200 OK
```

## Domain Events

### What Are Domain Events?

Domain events represent something important that happened in the business domain.

**Characteristics**:
- Immutable once created
- Named in past tense (Created, Updated, Deleted)
- Contain only relevant data
- Published within domain

**Example**:
```csharp
public class EmployeeCreatedDomainEvent : DomainEvent
{
    public int EmployeeId { get; set; }
    public string FirstName { get; set; }
    public string Email { get; set; }
    // ... other relevant data
}
```

### Publishing Domain Events

**Flow**:
```
1. Command Handler creates domain event
   new EmployeeCreatedDomainEvent { EmployeeId = 1, ... }

2. Event is published via MediatR
   await mediator.Publish(domainEvent)

3. All registered handlers are notified
   EmployeeCreatedDomainEventHandler.Handle(event)
   ↓
   Converts to integration event
   ↓
   Publishes to Service Bus
```

## Integration Events

### What Are Integration Events?

Integration events are used for communication between microservices.

**Differences from Domain Events**:
- Serializable to JSON
- Published to Service Bus
- Can span multiple services
- May include service-specific data

**Example**:
```csharp
public class EmployeeCreatedIntegrationEvent : IntegrationEvent
{
    public int EmployeeId { get; set; }
    public string Email { get; set; }
    public decimal Salary { get; set; }
    // ... service-specific data
}
```

### Publishing Integration Events

**Flow**:
```
Domain Event Handler
    ↓
Convert to Integration Event
    ↓
OutboxEventPublisher
    ├─ Store in OutboxMessages table
    ├─ Publish to Service Bus
    └─ Mark as processed on success
    ↓
If failed: Background service retries
```

## Outbox Pattern

### Problem It Solves

**Without Outbox Pattern** (Unreliable):
```
1. Save employee to DB → Success
2. Publish event to Service Bus → FAILS!
3. Event never sent (data loss)
```

**With Outbox Pattern** (Reliable):
```
1. Save employee + outbox message in SAME transaction
   ├─ Both succeed or both fail (atomic)
2. Publish to Service Bus
3. Mark outbox as processed on success
4. If step 2 fails:
   ├─ Background service detects unprocessed message
   ├─ Retries publish
   └─ Ensures eventual delivery
```

### Implementation

**Database Schema**:
```sql
CREATE TABLE OutboxMessages (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    AggregateId NVARCHAR(MAX),
    EventType NVARCHAR(256) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,  -- JSON serialized event
    CreatedAt DATETIME2 NOT NULL,
    ProcessedAt DATETIME2 NULL,
    RetryCount INT NOT NULL DEFAULT 0,
    IsProcessed BIT NOT NULL DEFAULT 0,
    Error NVARCHAR(MAX) NULL,
    INDEX IX_IsProcessed (IsProcessed),
    INDEX IX_CreatedAt (CreatedAt)
);
```

**Code Flow**:
```csharp
public async Task PublishAsync<T>(T @event) where T : IntegrationEvent
{
    // Step 1: Store in outbox
    var outboxMessage = new OutboxMessage
    {
        EventType = @event.EventType,
        Content = JsonSerializer.Serialize(@event)
    };
    await _outboxRepository.AddAsync(outboxMessage);
    
    // Step 2: Publish to Service Bus
    try {
        await _innerPublisher.PublishAsync(@event);
        
        // Step 3: Mark as processed
        outboxMessage.IsProcessed = true;
        outboxMessage.ProcessedAt = DateTime.UtcNow;
        await _outboxRepository.UpdateAsync(outboxMessage);
    }
    catch (Exception ex) {
        // Step 4: Will be retried by background service
        outboxMessage.Error = ex.Message;
        outboxMessage.RetryCount++;
        await _outboxRepository.UpdateAsync(outboxMessage);
    }
}
```

## Data Flow

### Complete Request-Response Cycle

```
┌─────────────────────────────────────────────────────────────┐
│ 1. HTTP Request                                             │
│ POST /api/v1/employees                                      │
│ { "firstName": "John", "lastName": "Doe", ... }           │
└────────────────┬────────────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────────────────┐
│ 2. EmployeesController                                      │
│ ├─ Receives CreateEmployeeCommand                           │
│ └─ Sends to MediatR                                         │
└────────────────┬────────────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────────────────┐
│ 3. MediatR Pipeline                                         │
│ ├─ CreateEmployeeCommandValidator                           │
│ │  └─ Validates input (email format, age, etc.)            │
│ └─ Creates CreateEmployeeCommandHandler instance           │
└────────────────┬────────────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────────────────┐
│ 4. CreateEmployeeCommandHandler.Handle()                    │
│ ├─ Check duplicate email via repository                    │
│ ├─ Create EmployeeEntity                                   │
│ └─ Save via EmployeeRepository.AddAsync()                  │
└────────────────┬────────────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────────────────┐
│ 5. EmployeeRepository.AddAsync()                            │
│ ├─ DbContext.Employees.AddAsync(employee)                  │
│ ├─ DbContext.SaveChangesAsync()                            │
│ │  └─ SQL: INSERT INTO Employees (...)                    │
│ └─ Return created entity                                   │
└────────────────┬────────────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────────────────┐
│ 6. Publish Domain Event                                     │
│ ├─ Create EmployeeCreatedDomainEvent                       │
│ └─ MediatR.Publish(domainEvent)                            │
└────────────────┬────────────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────────────────┐
│ 7. Domain Event Handler                                     │
│ ├─ EmployeeCreatedDomainEventHandler.Handle()              │
│ ├─ Convert to EmployeeCreatedIntegrationEvent              │
│ └─ PublishAsync(integrationEvent)                          │
└────────────────┬────────────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────────────────┐
│ 8. OutboxEventPublisher.PublishAsync()                      │
│ ├─ Create OutboxMessage                                    │
│ ├─ OutboxRepository.AddAsync(outboxMessage)               │
│ │  └─ SQL: INSERT INTO OutboxMessages (...)              │
│ ├─ ServiceBusEventPublisher.PublishAsync(event)            │
│ │  └─ ServiceBus.SendMessageAsync(message)                │
│ └─ Mark as processed on success                            │
└────────────────┬────────────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────────────────┐
│ 9. Map to DTO & Return Response                             │
│ ├─ EmployeeMapper.Map<EmployeeDto>(employee)               │
│ └─ HTTP 201 Created                                        │
└────────────────┬────────────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────────────────┐
│ 10. HTTP Response                                           │
│ 201 Created                                                 │
│ {"id": 1, "firstName": "John", ...}                       │
└─────────────────────────────────────────────────────────────┘
```

### Event Publishing in Service Bus

```
┌─────────────────────────────────┐
│ Employee Service Published      │
│ EmployeeCreatedIntegrationEvent │
└────────────┬────────────────────┘
             │
             ▼
  ┌──────────────────────────┐
  │ Azure Service Bus        │
  │ Topic: employee-events   │
  └──────────┬───────────────┘
             │
        ┌────┴──────┬─────────┬──────────┐
        │            │         │          │
        ▼            ▼         ▼          ▼
    Payment      Order       HR      Compliance
    Service      Service   Service    Service
        │            │         │          │
        └────┬───────┴─────────┴──────────┘
             │
             ▼
   PaymentServiceHandler
   OrderServiceHandler
   HRServiceHandler
   ComplianceServiceHandler
```

## Dependency Graph

```
Employee.API
    ├─ depends on
    ├─ Employee.Application
    │   ├─ depends on
    │   ├─ Employee.Domain
    │   │   ├─ (NO dependencies - pure domain)
    │   │   └─ Public interfaces:
    │   │       ├─ EmployeeEntity
    │   │       └─ DomainEvent
    │   ├─ depends on
    │   ├─ Employee.Domain
    │   └─ Public interfaces:
    │       ├─ Commands (CreateEmployeeCommand)
    │       ├─ Queries (GetEmployeeByIdQuery)
    │       ├─ DTOs (EmployeeDto)
    │       ├─ IEmployeeRepository (interface)
    │       └─ IEventPublisher (interface)
    └─ depends on
    └─ Employee.Infrastructure
        ├─ depends on
        ├─ Employee.Application
        ├─ depends on
        ├─ Employee.Domain
        └─ Public implementations:
            ├─ EmployeeRepository : IEmployeeRepository
            ├─ ServiceBusEventPublisher : IEventPublisher
            └─ EmployeeDbContext
```

## Database Schema

### Employees Table

```sql
CREATE TABLE Employees (
    Id INT PRIMARY KEY IDENTITY(1,1),
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PhoneNumber NVARCHAR(20) NOT NULL,
    Department NVARCHAR(100) NOT NULL,
    Position NVARCHAR(100) NOT NULL,
    Salary DECIMAL(18,2) NOT NULL,
    DateOfBirth DATETIME2 NOT NULL,
    HireDate DATETIME2 NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedBy NVARCHAR(255) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    ModifiedBy NVARCHAR(255),
    ModifiedAt DATETIME2,
    DeletedBy NVARCHAR(255),
    DeletedAt DATETIME2,
    IsDeleted BIT NOT NULL DEFAULT 0,
    INDEX IX_Email (Email),
    INDEX IX_Department (Department),
    INDEX IX_CreatedAt (CreatedAt),
    INDEX IX_IsDeleted_IsActive (IsDeleted, IsActive)
);
```

### OutboxMessages Table

```sql
CREATE TABLE OutboxMessages (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    AggregateId NVARCHAR(MAX),
    EventType NVARCHAR(256) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    ProcessedAt DATETIME2 NULL,
    RetryCount INT NOT NULL DEFAULT 0,
    IsProcessed BIT NOT NULL DEFAULT 0,
    Error NVARCHAR(MAX),
    INDEX IX_IsProcessed (IsProcessed),
    INDEX IX_CreatedAt (CreatedAt),
    INDEX IX_RetryCount (RetryCount)
);
```

---

**This architecture ensures:**
- ✅ Scalability (CQRS, Service Bus)
- ✅ Reliability (Outbox Pattern)
- ✅ Maintainability (Clean Architecture)
- ✅ Testability (Layered, Dependency Injection)
- ✅ Security (Authentication, Authorization)
- ✅ Observability (Logging, Health Checks)
