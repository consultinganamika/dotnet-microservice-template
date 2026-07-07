# Getting Started - Step by Step Guide

## 📋 Prerequisites Checklist

- [ ] .NET 8 SDK installed (`dotnet --version`)
- [ ] SQL Server or Docker installed
- [ ] Git installed
- [ ] Visual Studio 2022 or VS Code
- [ ] Admin access to install packages

## 🚀 Step-by-Step Execution Guide

### Phase 1: Project Setup (5 minutes)

#### Step 1.1: Clone Repository
```bash
git clone https://github.com/consultinganamika/dotnet-microservice-template.git
cd dotnet-microservice-template
```

#### Step 1.2: Open Solution
**Option A: Visual Studio 2022**
```bash
start EmployeeManagementSystem.sln
```

**Option B: VS Code**
```bash
code .
```

#### Step 1.3: Verify Structure
Confirm you see these folders:
```
src/
├── Employee.API
├── Employee.Application
├── Employee.Domain
└── Employee.Infrastructure
```

### Phase 2: Dependencies (10 minutes)

#### Step 2.1: Restore NuGet Packages
```bash
dotnet restore
```

#### Step 2.2: Verify Packages
```bash
# Check main packages
dotnet package search MediatR
dotnet package search EntityFrameworkCore
```

### Phase 3: Database Setup (15 minutes)

#### Step 3.1: Start SQL Server (Choose One)

**Option A: Docker (Recommended)**
```bash
# Start containers
docker-compose up -d

# Verify SQL Server is running
docker-compose ps
```

**Option B: Local SQL Server**
- Install SQL Server 2019 or later
- Update connection string in `appsettings.json`

#### Step 3.2: Create Database
```bash
# Navigate to Infrastructure project
cd src/Employee.Infrastructure

# Create migration
dotnet ef migrations add InitialCreate

# Apply migration
dotnet ef database update

# Go back to root
cd ../..
```

#### Step 3.3: Verify Database
```bash
# Query the database
# Using SSMS: Connect to localhost, check EmployeeDb
# Using CLI: sqlcmd -S localhost -U sa -P YourPassword123! -Q "SELECT * FROM Employees"
```

### Phase 4: Configure Application (5 minutes)

#### Step 4.1: Review Configuration
Open `src/Employee.API/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EmployeeDb;...",
    "ServiceBus": "Endpoint=sb://your-namespace.servicebus.windows.net/;..."
  }
}
```

#### Step 4.2: Local Development Settings
- For local dev, default settings should work
- JWT Secret is for demo only (change in production)
- Service Bus is optional for local testing

### Phase 5: Run Application (5 minutes)

#### Step 5.1: Start API
```bash
# From root directory
cd src/Employee.API
dotnet run
```

**Expected Output**:
```
Building...
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to stop.
```

#### Step 5.2: Access API
- **Swagger UI**: https://localhost:5001/swagger
- **Health Check**: https://localhost:5001/health
- **API Base**: https://localhost:5001/api/v1

### Phase 6: Test Application (10 minutes)

#### Step 6.1: Test via Swagger UI
1. Open https://localhost:5001/swagger
2. Expand "Employees" section
3. Click "Try it out" on GET /api/v1/employees
4. Click "Execute"
5. See response (should be empty array initially)

#### Step 6.2: Create Employee
1. Scroll to POST /api/v1/employees
2. Click "Try it out"
3. Modify request body:
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com",
  "phoneNumber": "+11234567890",
  "department": "Engineering",
  "position": "Developer",
  "salary": 100000,
  "dateOfBirth": "1990-01-01",
  "hireDate": "2023-01-01"
}
```
4. Click "Execute"
5. Verify 201 Created response

#### Step 6.3: Retrieve Employee
1. Scroll to GET /api/v1/employees/{id}
2. Enter ID: 1
3. Click "Execute"
4. Verify employee data returned

#### Step 6.4: Update Employee
1. Scroll to PUT /api/v1/employees/{id}
2. Enter ID: 1
3. Modify salary to 120000
4. Click "Execute"
5. Verify 200 OK response

#### Step 6.5: Delete Employee
1. Scroll to DELETE /api/v1/employees/{id}
2. Enter ID: 1
3. Click "Execute"
4. Verify 204 No Content response
5. Try GET again - should return 404

### Phase 7: Verify Logging & Events (5 minutes)

#### Step 7.1: Check Logs
```bash
# Logs are written to:
ls -la logs/

# View latest log
cat logs/app-*.txt
```

#### Step 7.2: Monitor Events
When you create/update/delete employees:
- Domain events are published internally
- Integration events are queued in Outbox table
- Background service would retry failed publishes

#### Step 7.3: Query Outbox
```sql
SELECT * FROM OutboxMessages ORDER BY CreatedAt DESC;
```

### Phase 8: Setup Azure Service Bus (Optional, 15 minutes)

#### Step 8.1: Prerequisites
- Azure subscription
- Azure CLI installed
- Admin access

#### Step 8.2: Create Service Bus
```bash
# Set variables
RESSOURCE_GROUP="myResourceGroup"
NAMESPACE="employee-service-ns"

# Create namespace
az servicebus namespace create \
  --resource-group $RESOURCE_GROUP \
  --name $NAMESPACE \
  --location eastus

# Create topic
az servicebus topic create \
  --resource-group $RESOURCE_GROUP \
  --namespace-name $NAMESPACE \
  --name employee-events

# Create subscription
az servicebus topic subscription create \
  --resource-group $RESOURCE_GROUP \
  --namespace-name $NAMESPACE \
  --topic-name employee-events \
  --name employee-service-subscription
```

#### Step 8.3: Get Connection String
```bash
az servicebus namespace authorization-rule keys list \
  --resource-group $RESOURCE_GROUP \
  --namespace-name $NAMESPACE \
  --name RootManageSharedAccessKey \
  --query primaryConnectionString \
  --output tsv
```

#### Step 8.4: Update Configuration
Update `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "ServiceBus": "Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=YOUR_KEY"
  }
}
```

#### Step 8.5: Restart Application
```bash
# Stop current instance (Ctrl+C)
# Restart
cd src/Employee.API
dotnet run
```

Events will now be published to Service Bus!

---

## 🎯 Quick Reference Commands

### Docker Commands
```bash
# Start containers
docker-compose up -d

# Stop containers
docker-compose down

# View logs
docker-compose logs -f

# Connect to SQL Server
docker exec -it <container-id> /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourPassword123!
```

### .NET Commands
```bash
# Run project
dotnet run

# Run with watch (auto-restart)
dotnet watch run

# Build
dotnet build

# Build Release
dotnet build -c Release

# Clean
dotnet clean

# Restore packages
dotnet restore
```

### Entity Framework Commands
```bash
# List migrations
dotnet ef migrations list

# Add migration
dotnet ef migrations add MigrationName

# Remove last migration
dotnet ef migrations remove

# Update database
dotnet ef database update

# Drop database
dotnet ef database drop

# Generate SQL script
dotnet ef migrations script
```

### Azure CLI Commands
```bash
# Login to Azure
az login

# List resources
az servicebus namespace list

# Create namespace
az servicebus namespace create --name ns-name --resource-group rg

# Create topic
az servicebus topic create --namespace-name ns --name topic-name --resource-group rg
```

---

## ✅ Verification Checklist

### After Setup Complete
- [ ] Solution opens in Visual Studio
- [ ] NuGet packages restored
- [ ] SQL Server container running (or local instance)
- [ ] Database migrations applied
- [ ] Application starts without errors
- [ ] Swagger UI accessible
- [ ] Can create employee via API
- [ ] Can retrieve employee via API
- [ ] Can update employee via API
- [ ] Can delete employee via API
- [ ] Health check endpoint works
- [ ] Logs are being written

### Optional: Service Bus
- [ ] Service Bus namespace created in Azure
- [ ] Topic and subscription created
- [ ] Connection string updated in config
- [ ] Application publishes events to Service Bus
- [ ] Events appear in subscription

---

## 🆘 Common Issues & Solutions

### Issue: "Connection Refused" to SQL Server
```bash
# Restart Docker
docker-compose restart

# Or verify local SQL Server is running
```

### Issue: "Port Already in Use"
```bash
# Run on different port
dotnet run --urls "https://localhost:5002"
```

### Issue: "Migration Failed"
```bash
# Reset database
cd src/Employee.Infrastructure
dotnet ef database drop -f
dotnet ef database update
cd ../..
```

### Issue: "Package Not Found"
```bash
# Clear NuGet cache and restore
dotnet nuget locals all --clear
dotnet restore
```

---

## 📚 Next Steps

1. **Explore the Code**
   - Understand layered architecture
   - Study CQRS implementation
   - Review event-driven patterns

2. **Add Unit Tests**
   - Create Employee.Tests project
   - Test handlers and validators
   - Use xUnit or NUnit

3. **Setup CI/CD**
   - GitHub Actions
   - Azure Pipelines
   - Docker Registry

4. **Deploy to Cloud**
   - Azure Container Instances
   - Azure App Service
   - Kubernetes (AKS)

5. **Integrate with Microservices**
   - Setup Payment Service
   - Setup Order Service
   - Setup HR Service
   - Enable inter-service communication

---

**Congratulations! Your microservice is now running! 🎉**
