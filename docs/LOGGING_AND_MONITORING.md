# Logging and Monitoring

## Logging Configuration

### Serilog Setup

Configured in `Program.cs` with multiple sinks:
- Console output
- File logging with rolling intervals
- Application Insights integration

## Log Levels

- **Trace** - Detailed diagnostic information
- **Debug** - Debug-level messages
- **Information** - Informational messages
- **Warning** - Warning messages
- **Error** - Error messages
- **Fatal** - Fatal error messages

## Structured Logging

```csharp
logger.LogInformation(
    "Processing employee {EmployeeId} with action {Action}",
    employeeId,
    "Create");
```

## Sensitive Data Masking

### Email Masking
```
user@company.com → user***@company.com
```

### Phone Masking
```
+1-555-123-4567 → +1-555-***-****
```

### PII Protection
- Social Security Numbers
- Credit Card Numbers
- Personal Identifiers

## Health Checks

### Endpoints

```
GET /health              - Basic health status
GET /health/detailed     - Detailed service health
```

### Health Check Monitors

- Database connectivity
- External API availability
- Message queue status
- Disk space availability
- Memory usage
- CPU usage
