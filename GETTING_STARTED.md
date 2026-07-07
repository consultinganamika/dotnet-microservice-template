# GETTING STARTED - Quick Setup Guide

> Get the Employee Management System running in 5-15 minutes!

## 📋 Prerequisites Checklist

Before you start, verify you have:

- [ ] .NET 8 SDK (`dotnet --version`)
- [ ] SQL Server or Docker installed
- [ ] Git installed  
- [ ] Visual Studio 2022 or VS Code
- [ ] Admin access to install packages

## 🚀 Phase 1: Clone & Open (2 minutes)

### Step 1.1: Clone Repository
```bash
git clone https://github.com/consultinganamika/dotnet-microservice-template.git
cd dotnet-microservice-template
```

### Step 1.2: Open in IDE

**Visual Studio 2022:**
```bash
start EmployeeManagementSystem.sln
```

**VS Code:**
```bash
code .
```

### Step 1.3: Verify Structure
Confirm you see:
```
src/
├── Employee.API
├── Employee.Application
├── Employee.Domain
└── Employee.Infrastructure
```

## 🔄 Phase 2: Dependencies (5 minutes)

### Step 2.1: Restore Packages
```bash
dotnet restore
```

**Output Should Show:**
```
Restore completed in xxx ms for EmployeeManagementSystem.sln
```

### Step 2.2: Verify Build
```bash
dotnet build
```

**Expected Output:**
```
Build succeeded. X files generated.
```

## 🗄️ Phase 3: Database Setup (10 minutes)

### Step 3.1: Start SQL Server

**Option A: Docker (Recommended)**
```bash
# Start containers
docker-compose up -d

# Verify running
docker-compose ps
```

**Expected Output:**
```
NAME       STATUS
sqlserver  Up 2 seconds
redis      Up 2 seconds
```

**Option B: Local SQL Server**
- Install SQL Server 2019 or later
- Update `appsettings.json` connection string

### Step 3.2: Create Migrations
```bash
cd src/Employee.Infrastructure
dotnet ef migrations add InitialCreate
```

**Expected Output:**
```
Added migration 'InitialCreate'.
```

### Step 3.3: Update Database
```bash
dotnet ef database update
cd ../..
```

**Expected Output:**
```
Building model for context 'EmployeeDbContext'.
Applying migration '..._InitialCreate'.
```

### Step 3.4: Verify Database
```sql
-- Check tables created
SELECT * FROM Employees;
SELECT * FROM OutboxMessages;
```

## ⚙️ Phase 4: Configuration (3 minutes)

### Step 4.1: Review Configuration
Open `src/Employee.API/appsettings.json`

**For Local Dev:**
- ✅ Default connection string works
- ✅ JWT secret is for demo
- ✅ Service Bus is optional

### Step 4.2: Optional: User Secrets
```bash
# Store sensitive data (won't be committed)
dotnet user-secrets init
dotnet user-secrets set "JwtSettings:Secret" "your-secret"
```

## 🎬 Phase 5: Run Application (2 minutes)

### Step 5.1: Start API
```bash
cd src/Employee.API
dotnet run
```

**Expected Output:**
```
Building...
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started.
```

### Step 5.2: Verify Running
Open in browser:
- **Swagger UI**: https://localhost:5001/swagger
- **Health Check**: https://localhost:5001/health
- **API Base**: https://localhost:5001/api/v1

## 🧪 Phase 6: Test Application (5 minutes)

### Step 6.1: Get All Employees
1. Open https://localhost:5001/swagger
2. Find "GET /api/v1/employees"
3. Click "Try it out" → "Execute"
4. See empty array: `{"data": [], ...}`

### Step 6.2: Create Employee
1. Find "POST /api/v1/employees"
2. Click "Try it out"
3. Paste request body:
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com",
  "phoneNumber": "+11234567890",
  "department": "Engineering",
  "position": "Senior Developer",
  "salary": 150000,
  "dateOfBirth": "1990-01-15",
  "hireDate": "2020-01-15"
}
```
4. Click "Execute"
5. Verify response code: **201 Created**

### Step 6.3: Get Employee by ID
1. Find "GET /api/v1/employees/{id}"
2. Enter ID: `1`
3. Click "Execute"
4. Verify response includes your employee

### Step 6.4: Update Employee
1. Find "PUT /api/v1/employees/{id}"
2. Enter ID: `1`
3. Modify salary: `160000`
4. Click "Execute"
5. Verify response code: **200 OK**

### Step 6.5: Delete Employee
1. Find "DELETE /api/v1/employees/{id}"
2. Enter ID: `1`
3. Click "Execute"
4. Verify response code: **204 No Content**
5. Try GET again - should return 404

## 📊 Phase 7: Verify Events & Logging (3 minutes)

### Step 7.1: Check Logs
```bash
ls -la logs/
cat logs/app-*.txt
```

You should see:
```
info: Employee created
info: Integration event published
info: Domain event handled
```

### Step 7.2: Query Outbox Table
```sql
SELECT * FROM OutboxMessages ORDER BY CreatedAt DESC;
```

Should show your published events.

### Step 7.3: Monitor Background Service
In application logs, look for:
```
Outbox Publisher Background Service started
Processing outbox message: ...
```

## ☁️ Phase 8: Azure Service Bus Setup (Optional, 15 minutes)

### Step 8.1: Create Service Bus
```bash
az login
az servicebus namespace create \
  --resource-group myRg \
  --name employee-service-ns \
  --location eastus
```

### Step 8.2: Create Topic
```bash
az servicebus topic create \
  --resource-group myRg \
  --namespace-name employee-service-ns \
  --name employee-events
```

### Step 8.3: Create Subscription
```bash
az servicebus topic subscription create \
  --resource-group myRg \
  --namespace-name employee-service-ns \
  --topic-name employee-events \
  --name employee-service-subscription
```

### Step 8.4: Get Connection String
```bash
az servicebus namespace authorization-rule keys list \
  --resource-group myRg \
  --namespace-name employee-service-ns \
  --name RootManageSharedAccessKey \
  --query primaryConnectionString \
  --output tsv
```

### Step 8.5: Update Configuration
Update `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "ServiceBus": "Endpoint=sb://employee-service-ns.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=YOUR_KEY"
  }
}
```

### Step 8.6: Restart Application
```bash
# Stop current (Ctrl+C)
# Restart
cd src/Employee.API
dotnet run
```

Events now publish to Service Bus! 🎉

---

## 🎯 Quick Reference Commands

### Docker
```bash
docker-compose up -d          # Start
docker-compose down           # Stop
docker-compose logs -f        # View logs
```

### .NET
```bash
dotnet run                    # Run
dotnet watch run              # Run with auto-restart
dotnet build                  # Build
dotnet clean                  # Clean
```

### Entity Framework
```bash
cd src/Employee.Infrastructure
dotnet ef migrations add Name      # Create migration
dotnet ef migrations remove        # Remove last
dotnet ef database update          # Apply migrations
dotnet ef database drop -f         # Delete database
```

### Azure CLI
```bash
az login                           # Login to Azure
az servicebus namespace list       # List namespaces
az group create -n myRg -l eastus  # Create resource group
```

---

## ✅ Verification Checklist

After completing all phases:

- [ ] Solution opens in IDE
- [ ] All 4 projects load
- [ ] Packages restored
- [ ] Docker containers running
- [ ] Database migrations applied
- [ ] Application starts without errors
- [ ] Swagger UI accessible
- [ ] Can create employee
- [ ] Can retrieve employee
- [ ] Can update employee
- [ ] Can delete employee
- [ ] Health check works (/health)
- [ ] Logs are being written
- [ ] Database has Employees table
- [ ] Outbox table exists

## 🆘 Common Issues & Solutions

### "Connection refused" to SQL Server
```bash
docker-compose restart
# or verify local SQL Server is running
```

### "Port 5001 already in use"
```bash
dotnet run --urls "https://localhost:5002"
```

### "EF Migration error"
```bash
cd src/Employee.Infrastructure
dotnet ef database drop -f
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### "Cannot find SQL Server"
```bash
# Check appsettings.json connection string
# For Docker: Server=localhost
# For local: Server=YOUR_SERVER_NAME
```

### "NuGet restore failed"
```bash
dotnet nuget locals all --clear
dotnet restore
```

### "Package not found"
```bash
# Check internet connection
# Try different NuGet source
dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org
```

---

## 📖 Next Steps

1. ✅ **Completed Setup?** 
   → Explore the codebase structure

2. **Understand Architecture?**
   → Read [docs/ARCHITECTURE.md](./docs/ARCHITECTURE.md)

3. **Want Full Overview?**
   → Check [COMPREHENSIVE_README.md](./COMPREHENSIVE_README.md)

4. **Setup Azure Service Bus?**
   → Follow [SERVICE_BUS_SETUP.md](./SERVICE_BUS_SETUP.md)

5. **Add Unit Tests?**
   → Create Employee.Tests project with xUnit

6. **Deploy to Cloud?**
   → Use Docker and Azure Container Registry

---

**🎉 Congratulations! Your microservice is running!**

📊 **Total Setup Time**: 5-15 minutes  
✅ **Status**: Ready for Development  
🚀 **Next**: Start exploring the code!
