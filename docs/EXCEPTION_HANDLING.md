# Global Exception Handling

## Exception Hierarchy

### Custom Exceptions

```
ApplicationException (Base)
├── NotFoundException
├── ValidationException
├── UnauthorizedException
├── ForbiddenException
├── ConflictException
├── DownstreamServiceException
├── InternalServerException
└── NotImplementedException
```

## Exception Handling Middleware

The middleware intercepts all exceptions and maps them to appropriate HTTP responses.

### Exception Mapping

```
NotFoundException         → 404 Not Found
ValidationException      → 400 Bad Request
UnauthorizedException    → 401 Unauthorized
ForbiddenException       → 403 Forbidden
ConflictException        → 409 Conflict
DownstreamServiceException → 502 Bad Gateway
InternalServerException  → 500 Internal Server Error
NotImplementedException  → 501 Not Implemented
```

## Standard Error Response

```json
{
  "type": "https://api.example.com/errors/not-found",
  "title": "Not Found",
  "status": 404,
  "detail": "Employee with ID 123 not found",
  "instance": "/api/v1/employees/123",
  "traceId": "0HN1GC7JG8K2L:00000001"
}
```

## Exception Handling Best Practices

1. **Catch Specific Exceptions** - Don't catch generic Exception
2. **Provide Context** - Include relevant data in messages
3. **Log Appropriately** - Use correct log level
4. **User-Friendly Messages** - Don't expose internal details
5. **Prevent Information Disclosure** - Hide sensitive information
6. **Consistency** - Use standard error response format
