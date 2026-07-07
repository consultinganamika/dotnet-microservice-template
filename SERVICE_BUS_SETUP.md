# Azure Service Bus & Inter-Microservice Communication Setup

## Overview

This document provides comprehensive setup and usage instructions for Azure Service Bus integration and inter-microservice communication in the Employee Management Microservice.

## Architecture

### Event Flow

```
┌──────────────────────────────────────┐
│      Employee Service                │
│  (Publishes & Receives Events)       │
└──────────────┬───────────────────────┘
               │
               ├─ Publishes: EmployeeCreated, EmployeeUpdated, etc.
               │
               ▼
┌──────────────────────────────────────┐
│    Azure Service Bus                 │
│  (Topic: employee-events)            │
└──────────────┬───────────────────────┘
               │
        ┌──────┼──────┬──────────┬─────────┐
        ▼      ▼      ▼          ▼         ▼
    Payment  Order   HR      Compliance  Audit
    Service  Service Service  Service   Service
```

### Outbox Pattern

```
┌─────────────────────────────────────────┐
│ Employee Operation (Create/Update)      │
└──────────────────┬──────────────────────┘
                   │
        ┌──────────┴──────────┐
        ▼                     ▼
   Database               Outbox Table
  (Employee)          (OutboxMessage)
        │                     │
        │                     ▼
        │            Service Bus Publish
        │                     │
        │          (if failed, retry)
        │                     │
        └─────────────────────┘
            Background Service
            (Processes retries
             every 30 seconds)
```

## Prerequisites

- Azure subscription
- Azure CLI or Azure Portal access
- .NET 8+
- SQL Server 2019+

## Setup Instructions

### 1. Create Azure Service Bus Namespace

```bash
# Set variables
RESOURCE_GROUP="myResourceGroup"
NAMESPACE="employee-service-ns"
LOCATION="eastus"

# Create resource group (if not exists)
az group create \
  --name $RESOURCE_GROUP \
  --location $LOCATION

# Create Service Bus namespace
az servicebus namespace create \
  --resource-group $RESOURCE_GROUP \
  --name $NAMESPACE \
  --location $LOCATION \
  --sku Standard
```

### 2. Create Topic and Subscriptions

```bash
# Create topic
az servicebus topic create \
  --resource-group $RESOURCE_GROUP \
  --namespace-name $NAMESPACE \
  --name employee-events \
  --max-size 1073741824 \
  --default-message-ttl P0DT0H30M0S

# Create subscription for Employee Service
az servicebus topic subscription create \
  --resource-group $RESOURCE_GROUP \
  --namespace-name $NAMESPACE \
  --topic-name employee-events \
  --name employee-service-subscription

# Create subscriptions for other services
for service in payment order hr compliance
do
  az servicebus topic subscription create \
    --resource-group $RESOURCE_GROUP \
    --namespace-name $NAMESPACE \
    --topic-name employee-events \
    --name ${service}-service-subscription
done
```

### 3. Get Connection String

```bash
# Get primary connection string
CONNECTION_STRING=$(az servicebus namespace authorization-rule keys list \
  --resource-group $RESOURCE_GROUP \
  --namespace-name $NAMESPACE \
  --name RootManageSharedAccessKey \
  --query primaryConnectionString \
  --output tsv)

echo "Connection String: $CONNECTION_STRING"
```

### 4. Update Application Configuration

Update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "ServiceBus": "Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=your-key"
  },
  "ServiceBusSettings": {
    "TopicName": "employee-events",
    "SubscriptionName": "employee-service-subscription"
  }
}
```

### 5. Create Outbox Table Migration

```bash
cd src/Employee.Infrastructure
dotnet ef migrations add AddOutboxTable
dotnet ef database update
```

Or manually create the table:

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
    Error NVARCHAR(MAX) NULL,
    INDEX IX_OutboxMessages_IsProcessed (IsProcessed),
    INDEX IX_OutboxMessages_CreatedAt (CreatedAt)
);
```

## Usage Examples

### Publishing Employee Events

```csharp
public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, EmployeeDto>
{
    private readonly IEmployeeRepository _repository;
    private readonly IMediator _mediator;
    private readonly IEventPublisher _eventPublisher;

    public async Task<EmployeeDto> Handle(
        CreateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        // Create employee
        var employee = new EmployeeEntity
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            // ... other properties
        };

        await _repository.AddAsync(employee, cancellationToken);

        // Publish domain event (automatically converted to integration event)
        var domainEvent = new EmployeeCreatedDomainEvent
        {
            EmployeeId = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            Department = employee.Department,
            Position = employee.Position,
            Salary = employee.Salary,
            HireDate = employee.HireDate
        };

        await _mediator.Publish(domainEvent, cancellationToken);

        return _mapper.Map<EmployeeDto>(employee);
    }
}
```

### Handling External Service Events

```csharp
public class PaymentProcessedEventHandler : INotificationHandler<PaymentProcessedEvent>
{
    private readonly ILogger<PaymentProcessedEventHandler> _logger;

    public async Task Handle(
        PaymentProcessedEvent notification,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Payment processed for employee {EmployeeId}: Amount={Amount}",
            notification.EmployeeId,
            notification.Amount);

        // Implement business logic
        // - Update employee payment status
        // - Send confirmation email
        // - Update accounting records
        // - Trigger dependent workflows

        await Task.CompletedTask;
    }
}
```

### Publishing Batch Events

```csharp
var events = new List<EmployeeCreatedIntegrationEvent>
{
    new EmployeeCreatedIntegrationEvent { /* ... */ },
    new EmployeeCreatedIntegrationEvent { /* ... */ },
    new EmployeeCreatedIntegrationEvent { /* ... */ }
};

await _eventPublisher.PublishBatchAsync(events, cancellationToken);
```

## Event Types

### Employee Service Events

#### EmployeeCreatedIntegrationEvent
```csharp
public class EmployeeCreatedIntegrationEvent : IntegrationEvent
{
    public int EmployeeId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Department { get; set; }
    public string Position { get; set; }
    public decimal Salary { get; set; }
    public DateTime HireDate { get; set; }
}
```

#### EmployeeUpdatedIntegrationEvent
```csharp
public class EmployeeUpdatedIntegrationEvent : IntegrationEvent
{
    public int EmployeeId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Department { get; set; }
    public string Position { get; set; }
    public decimal Salary { get; set; }
    public bool IsActive { get; set; }
}
```

#### EmployeeDeletedIntegrationEvent
```csharp
public class EmployeeDeletedIntegrationEvent : IntegrationEvent
{
    public int EmployeeId { get; set; }
    public string Email { get; set; }
    public DateTime DeletedAt { get; set; }
}
```

### External Service Events

**Payment Service:**
- `PaymentProcessedEvent` - Payment completed
- `PaymentFailedEvent` - Payment failed
- `SalaryAdjustmentEvent` - Salary adjusted

**Order Service:**
- `OrderCreatedEvent` - New order created
- `OrderShippedEvent` - Order shipped
- `OrderCancelledEvent` - Order cancelled

**HR Service:**
- `AttendanceRecordedEvent` - Attendance recorded
- `LeaveRequestApprovedEvent` - Leave approved

**Compliance Service:**
- `AuditLogCreatedEvent` - Audit log created
- `ComplianceViolationDetectedEvent` - Violation detected

## Commands to Other Microservices

### Payment Service Commands
- `ProcessEmployeePaymentCommand` - Initiate payment
- `UpdateEmployeeSalaryCommand` - Update salary

### Order Service Commands
- `CancelEmployeeOrdersCommand` - Cancel orders
- `UpdateEmployeeInOrderServiceCommand` - Update employee info

### HR Service Commands
- `CreateEmployeeHRRecordCommand` - Create HR record
- `DeactivateEmployeeHRRecordCommand` - Deactivate employee

### Compliance Service Commands
- `LogEmployeeAuditCommand` - Log audit entry
- `CheckEmployeeComplianceCommand` - Check compliance

## Monitoring and Troubleshooting

### Monitor Service Bus

```bash
# Check subscription metrics
az servicebus topic subscription show \
  --resource-group $RESOURCE_GROUP \
  --namespace-name $NAMESPACE \
  --topic-name employee-events \
  --name employee-service-subscription
```

### Check Outbox Messages

```sql
-- View all outbox messages
SELECT * FROM OutboxMessages ORDER BY CreatedAt DESC;

-- View failed messages
SELECT * FROM OutboxMessages WHERE IsProcessed = 0 AND RetryCount >= 5;

-- View retry statistics
SELECT 
    EventType,
    COUNT(*) as Count,
    AVG(RetryCount) as AvgRetries,
    MAX(RetryCount) as MaxRetries
FROM OutboxMessages
WHERE IsProcessed = 0
GROUP BY EventType;
```

### Common Issues

**Issue: "Connection refused" when connecting to Service Bus**
- Solution: Verify connection string is correct
- Solution: Check firewall rules allow your IP
- Solution: Ensure Service Bus namespace exists

**Issue: Messages not appearing in subscription**
- Solution: Verify topic and subscription names
- Solution: Check subscription filters
- Solution: Review application logs

**Issue: High retry count for outbox messages**
- Solution: Check Service Bus connectivity
- Solution: Review error messages in OutboxMessage.Error column
- Solution: Check application logs for detailed errors

## Performance Tuning

### Concurrency Settings

```json
{
  "ServiceBusSettings": {
    "MaxConcurrentCalls": 10,
    "PrefetchCount": 100
  }
}
```

### Batch Publishing

```csharp
// More efficient than publishing individually
await _eventPublisher.PublishBatchAsync(events, cancellationToken);
```

### Message TTL

Set appropriate message time-to-live in topic configuration:

```bash
az servicebus topic update \
  --resource-group $RESOURCE_GROUP \
  --namespace-name $NAMESPACE \
  --name employee-events \
  --default-message-ttl P0DT0H30M0S  # 30 minutes
```

## Security Best Practices

1. **Use Azure Key Vault**
   ```bash
   az keyvault secret set \
     --vault-name myKeyVault \
     --name ServiceBusConnectionString \
     --value "$CONNECTION_STRING"
   ```

2. **Use Managed Identities**
   ```csharp
   var credential = new DefaultAzureCredential();
   var client = new ServiceBusClient(
       "your-namespace.servicebus.windows.net",
       credential);
   ```

3. **Implement Message Encryption**
   - Encrypt sensitive data before publishing
   - Use TLS for all Service Bus connections

4. **Enable Firewall**
   ```bash
   az servicebus namespace update \
     --resource-group $RESOURCE_GROUP \
     --name $NAMESPACE \
     --default-action Deny
   ```

5. **Separate Access Keys**
   - Use different keys for each service
   - Rotate keys regularly

## Deployment

### Docker Compose

For local development with SQL Server:

```yaml
version: '3.8'
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: "YourPassword123!"
      ACCEPT_EULA: "Y"
    ports:
      - "1433:1433"

  app:
    build: .
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=EmployeeDb;User Id=sa;Password=YourPassword123!;
      - ConnectionStrings__ServiceBus=<your-service-bus-connection-string>
    ports:
      - "5000:5000"
    depends_on:
      - sqlserver
```

### Kubernetes

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: employee-service-config
data:
  ServiceBusSettings__TopicName: employee-events
  ServiceBusSettings__SubscriptionName: employee-service-subscription

---
apiVersion: v1
kind: Secret
metadata:
  name: employee-service-secrets
type: Opaque
stringData:
  ConnectionStrings__ServiceBus: <base64-encoded-connection-string>
```

## Support and Resources

- [Azure Service Bus Documentation](https://docs.microsoft.com/en-us/azure/service-bus-messaging/)
- [Outbox Pattern](https://microservices.io/patterns/data/transactional-outbox.html)
- [Azure CLI Reference](https://docs.microsoft.com/en-us/cli/azure/)
