# Employee Management System - .NET Microservice

> Enterprise-grade .NET 8+ microservice template demonstrating Clean Architecture, CQRS pattern, event-driven design, and inter-microservice communication using Azure Service Bus.

## 🌟 Quick Links

- 📖 **[Comprehensive Guide](./COMPREHENSIVE_README.md)** - Full documentation
- 🚀 **[Getting Started](./GETTING_STARTED.md)** - Step-by-step setup (5-15 minutes)
- 🏛️ **[Architecture](./docs/ARCHITECTURE.md)** - Deep dive into design
- ☁️ **[Service Bus Setup](./SERVICE_BUS_SETUP.md)** - Azure integration guide

## 📋 What's Inside?

A complete, production-ready microservice with:

✅ **Clean Architecture** - 4-layer separation of concerns  
✅ **CQRS Pattern** - Separate read and write operations  
✅ **Domain Events** - Rich domain modeling  
✅ **Integration Events** - Inter-microservice communication via Azure Service Bus  
✅ **Outbox Pattern** - Guaranteed event delivery with retry  
✅ **API Versioning** - Multiple API versions supported  
✅ **JWT Authentication** - Secure endpoints  
✅ **Role-Based Authorization** - Fine-grained access control  
✅ **Swagger/OpenAPI** - Auto-generated API documentation  
✅ **Structured Logging** - Serilog integration  
✅ **Health Checks** - Application monitoring  
✅ **Docker Support** - Container-ready  

## 🚀 30-Second Quick Start

```bash
# Clone
git clone https://github.com/consultinganamika/dotnet-microservice-template.git
cd dotnet-microservice-template

# Setup database
docker-compose up -d
cd src/Employee.Infrastructure
dotnet ef migrations add InitialCreate
dotnet ef database update
cd ../..

# Run
cd src/Employee.API
dotnet run

# Open https://localhost:5001/swagger
```

## 📁 Project Structure

```
src/
├── Employee.API              # REST API endpoints
├── Employee.Application      # CQRS, business logic
├── Employee.Domain           # Entities, domain events
└── Employee.Infrastructure   # Data access, Service Bus
```

## 🏗️ Architecture Overview

```
┌─────────────────────────────────────┐
│        API Layer (REST)             │
│   Controllers, Middleware, Auth     │
└────────────┬────────────────────────┘
             │
┌────────────▼────────────────────────┐
│   Application Layer (CQRS)          │
│   Commands, Queries, Validators     │
└────────────┬────────────────────────┘
             │
┌────────────▼────────────────────────┐
│      Domain Layer                   │
│   Entities, Business Rules          │
└────────────┬────────────────────────┘
             │
┌────────────▼────────────────────────┐
│  Infrastructure Layer               │
│  Database, Service Bus, Logging     │
└─────────────────────────────────────┘
```

## 📡 Inter-Microservice Communication

```
Employee Service
    ├─ Publishes: EmployeeCreated
    ├─ Publishes: EmployeeUpdated  
    ├─ Publishes: EmployeeSalary Updated
    └─ Consumes: Payment, Order, HR events
         ↓
    Azure Service Bus (Topic: employee-events)
         ↓
    ┌────┬──────┬──────┬───────────┐
    ↓    ↓      ↓      ↓           ↓
 Payment Order  HR  Compliance  Audit
 Service Service Service Service Service
```

## 🔧 Technology Stack

| Layer | Technology |
|-------|------------|
| **Framework** | .NET 8+ |
| **Database** | SQL Server 2019+ |
| **ORM** | Entity Framework Core 8.0 |
| **Message Bus** | Azure Service Bus |
| **CQRS** | MediatR |
| **Validation** | FluentValidation |
| **Mapping** | AutoMapper |
| **Logging** | Serilog |
| **API Docs** | Swagger/OpenAPI |
| **Caching** | Redis/In-Memory |

## 📚 Key Features

### 1. Clean Architecture
- Clear layer separation
- No circular dependencies
- Easy to test and maintain

### 2. CQRS Pattern
- Separate commands (write) and queries (read)
- Optimized data access
- Better scalability

### 3. Domain Events
- Domain-driven design
- Asynchronous processing
- Loose coupling

### 4. Integration Events
- Inter-microservice communication
- Async event publishing via Service Bus
- Guaranteed delivery (Outbox pattern)

### 5. Outbox Pattern
- Reliable event delivery
- Retry mechanism
- No message loss

### 6. API Security
- JWT authentication
- Role-based authorization
- CORS policy

## 🎯 API Endpoints

```
GET    /api/v1/employees              # Get all employees (paginated)
GET    /api/v1/employees/{id}         # Get employee by ID
POST   /api/v1/employees              # Create employee
PUT    /api/v1/employees/{id}         # Update employee
DELETE /api/v1/employees/{id}         # Delete employee
GET    /health                        # Health check
```

## 📖 Documentation

### For Quick Setup
👉 **[GETTING_STARTED.md](./GETTING_STARTED.md)**
- 8 phases from 0 to running
- Exact commands to execute
- Verification checklist
- Troubleshooting guide

### For Understanding Architecture
👉 **[docs/ARCHITECTURE.md](./docs/ARCHITECTURE.md)**
- Layer-by-layer breakdown
- CQRS pattern explained
- Domain vs Integration events
- Outbox pattern visualization
- Data flow diagrams
- Database schema

### For Comprehensive Overview
👉 **[COMPREHENSIVE_README.md](./COMPREHENSIVE_README.md)**
- Complete feature list
- All technologies explained
- Deployment guide
- Testing strategies
- Production best practices

### For Azure Service Bus
👉 **[SERVICE_BUS_SETUP.md](./SERVICE_BUS_SETUP.md)**
- Azure configuration
- Event types reference
- Inter-service communication
- Monitoring guide
- Security best practices

## 🛠️ Prerequisites

- ✅ .NET 8 SDK or later
- ✅ SQL Server 2019+ (or Docker)
- ✅ Visual Studio 2022 or VS Code
- ✅ Git
- ✅ Docker (optional but recommended)

## ⚡ Quick Commands

```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run migrations
cd src/Employee.Infrastructure
dotnet ef migrations add MigrationName
dotnet ef database update

# Run application
cd ../Employee.API
dotnet run

# Run with watch (auto-restart)
dotnet watch run

# Build Docker image
docker build -t employee-api:latest .

# Run Docker container
docker run -p 5000:5000 -p 5001:5001 employee-api:latest
```

## 🧪 Testing

### Via Swagger UI
1. Navigate to `https://localhost:5001/swagger`
2. Expand any endpoint
3. Click "Try it out"
4. Modify request if needed
5. Click "Execute"

### Via cURL
```bash
# Create employee
curl -X POST https://localhost:5001/api/v1/employees \
  -H "Content-Type: application/json" \
  -d '{"firstName":"John","lastName":"Doe","email":"john@example.com",...}'

# Get all employees
curl https://localhost:5001/api/v1/employees
```

### Via Postman
1. Import Swagger: `/swagger/v1/swagger.json`
2. Create environment with `base_url` variable
3. Run collection tests

## 📊 Events Supported

### Employee Service Events
- `EmployeeCreatedIntegrationEvent` - Employee created
- `EmployeeUpdatedIntegrationEvent` - Employee updated
- `EmployeeDeletedIntegrationEvent` - Employee deleted
- `EmployeeSalaryUpdatedIntegrationEvent` - Salary changed

### External Service Events (Consumed)
- **Payment**: Processed, Failed, Salary Adjustment
- **Order**: Created, Shipped, Cancelled
- **HR**: Attendance, Leave Approved
- **Compliance**: Audit, Violations

## 🔐 Security Features

- 🔒 JWT token authentication
- 👥 Role-based authorization (Admin, Manager, Employee, Support)
- 🛡️ CORS policy configuration
- 📝 Sensitive data masking
- 🔑 Secure secret management
- 🚨 Global error handling

## 📈 Scalability

- **Horizontal Scaling**: Stateless API design
- **Caching**: In-memory and Redis support
- **Event-Driven**: Async processing via Service Bus
- **CQRS**: Separate read/write optimization
- **Outbox Pattern**: Guaranteed event delivery

## 🐳 Docker Support

```bash
# Start development environment
docker-compose up -d

# Build production image
docker build -t employee-api:1.0.0 .

# Run container
docker run -e ConnectionStrings__DefaultConnection=... \
           -e ConnectionStrings__ServiceBus=... \
           -p 5000:5000 \
           employee-api:1.0.0
```

## 🚀 Deployment

### Azure Container Instances
```bash
az container create --resource-group myRg \
  --name employee-api \
  --image employee-api:latest \
  --cpu 1 --memory 1.5
```

### Azure App Service
```bash
az webapp up --name employee-api --resource-group myRg
```

### Kubernetes
```bash
kubectl apply -f deployment.yaml
kubectl apply -f service.yaml
```

## 📝 Environment Configuration

### Development (appsettings.Development.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EmployeeDb;...",
    "ServiceBus": "Endpoint=sb://namespace.servicebus.windows.net/;..."
  },
  "JwtSettings": {
    "Secret": "dev-secret-key",
    "ExpirationMinutes": 120
  }
}
```

### Production (Use Key Vault)
```bash
az keyvault secret set --vault-name myKeyVault \
  --name ConnectionStrings--DefaultConnection \
  --value "your-connection-string"
```

## 🐛 Troubleshooting

### "Connection Refused" to SQL Server
```bash
docker-compose restart
# or
verify SQL Server is running locally
```

### "Port Already in Use"
```bash
dotnet run --urls "https://localhost:5002"
```

### "EF Migrations Error"
```bash
cd src/Employee.Infrastructure
dotnet ef database drop -f
dotnet ef database update
```

### "NuGet Package Error"
```bash
dotnet nuget locals all --clear
dotnet restore
```

## 📚 Learning Resources

- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [Event Sourcing](https://martinfowler.com/eaaDev/EventSourcing.html)
- [Microservices](https://martinfowler.com/articles/microservices.html)
- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Azure Service Bus](https://docs.microsoft.com/en-us/azure/service-bus-messaging/)
- [MediatR](https://github.com/jbogard/MediatR)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Open a Pull Request

## 📄 License

MIT License - See LICENSE file for details

## 👥 Support

- 📧 Email: support@company.com
- 💬 Issues: GitHub Issues
- 📖 Wiki: GitHub Wiki

---

**Status**: ✅ Production Ready  
**Version**: 1.0.0  
**Last Updated**: July 2026  
**Maintained By**: ConsultingAnamika  

### Next Steps
1. ⭐ Star the repository
2. 👉 [Read Getting Started Guide](./GETTING_STARTED.md)
3. 🚀 [Run Quick Start Commands](./GETTING_STARTED.md#quick-start-command-summary)
4. 📖 [Explore Architecture](./docs/ARCHITECTURE.md)
5. 🔧 [Setup Azure Service Bus](./SERVICE_BUS_SETUP.md) (Optional)
