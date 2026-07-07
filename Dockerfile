FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Employee.API/Employee.API.csproj", "src/Employee.API/"]
COPY ["src/Employee.Application/Employee.Application.csproj", "src/Employee.Application/"]
COPY ["src/Employee.Domain/Employee.Domain.csproj", "src/Employee.Domain/"]
COPY ["src/Employee.Infrastructure/Employee.Infrastructure.csproj", "src/Employee.Infrastructure/"]

RUN dotnet restore "src/Employee.API/Employee.API.csproj"

COPY . .
WORKDIR "/src/src/Employee.API"
RUN dotnet build "Employee.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Employee.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Employee.API.dll"]
