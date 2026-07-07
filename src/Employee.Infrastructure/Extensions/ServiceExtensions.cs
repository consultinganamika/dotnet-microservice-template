using Employee.Application.Employees;
using Employee.Infrastructure.Data;
using Employee.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Employee.Infrastructure.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register DbContext
            services.AddDbContext<EmployeeDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
                
                options.UseSqlServer(
                    connectionString,
                    b => b.MigrationsAssembly(typeof(EmployeeDbContext).Assembly.FullName));
            });

            // Register repositories
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();

            return services;
        }
    }
}
