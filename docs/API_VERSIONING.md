# API Versioning Strategy

## Versioning Approach

This template implements URL-based API versioning:

```
/api/v1/employees     - Version 1 endpoints
/api/v2/employees     - Version 2 endpoints
```

## URL Routing

```csharp
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class EmployeeController : ControllerBase
{
    // Version 1 endpoints
}
```

## Swagger Integration

- Automatic versioning in Swagger UI
- Separate documentation per version
- Version selection dropdown

## Deprecation Strategy

1. **Announcement** - Communicate deprecation date
2. **Support Period** - Maintain old version for 12 months
3. **Migration** - Provide migration guide
4. **Sunset** - Remove deprecated version
