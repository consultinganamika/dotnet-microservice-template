using Employee.Application.Common.Models;
using Employee.Application.Employees;
using Employee.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Employee.Infrastructure.Data.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly EmployeeDbContext _context;
        private readonly ILogger<EmployeeRepository> _logger;

        public EmployeeRepository(EmployeeDbContext context, ILogger<EmployeeRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<EmployeeEntity> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Employees
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee by ID: {EmployeeId}", id);
                throw;
            }
        }

        public async Task<EmployeeEntity> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Employees
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Email == email && !e.IsDeleted, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee by email: {Email}", email);
                throw;
            }
        }

        public async Task<PaginatedResult<EmployeeEntity>> GetAllAsync(
            int pageNumber,
            int pageSize,
            string searchTerm = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _context.Employees
                    .AsNoTracking()
                    .Where(e => !e.IsDeleted)
                    .OrderByDescending(e => e.CreatedAt);

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(e =>
                        e.FirstName.Contains(searchTerm) ||
                        e.LastName.Contains(searchTerm) ||
                        e.Email.Contains(searchTerm) ||
                        e.Department.Contains(searchTerm));
                }

                var totalCount = await query.CountAsync(cancellationToken);

                var data = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                return new PaginatedResult<EmployeeEntity>
                {
                    Data = data,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all employees");
                throw;
            }
        }

        public async Task<EmployeeEntity> AddAsync(EmployeeEntity employee, CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.Employees.AddAsync(employee, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Employee added successfully with ID: {EmployeeId}", employee.Id);
                return employee;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding employee");
                throw;
            }
        }

        public async Task<EmployeeEntity> UpdateAsync(EmployeeEntity employee, CancellationToken cancellationToken = default)
        {
            try
            {
                _context.Employees.Update(employee);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Employee updated successfully with ID: {EmployeeId}", employee.Id);
                return employee;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating employee with ID: {EmployeeId}", employee.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(new object[] { id }, cancellationToken);
                if (employee == null)
                    return false;

                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Employee deleted successfully with ID: {EmployeeId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting employee with ID: {EmployeeId}", id);
                throw;
            }
        }
    }
}
