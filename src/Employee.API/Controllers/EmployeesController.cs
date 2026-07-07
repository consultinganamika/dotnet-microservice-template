using Asp.Versioning;
using Employee.Application.Employees.Commands.CreateEmployee;
using Employee.Application.Employees.Commands.DeleteEmployee;
using Employee.Application.Employees.Commands.UpdateEmployee;
using Employee.Application.Employees.Queries.GetAllEmployees;
using Employee.Application.Employees.Queries.GetEmployeeById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Employee.API.Controllers
{
    /// <summary>
    /// Employee management endpoints
    /// </summary>
    [Authorize]
    public class EmployeesController : BaseController
    {
        private readonly IMediator _mediator;

        public EmployeesController(ILogger<EmployeesController> logger, IMediator mediator)
            : base(logger)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all employees with pagination
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAllEmployees(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string searchTerm = null,
            CancellationToken cancellationToken = default)
        {
            var query = new GetAllEmployeesQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm
            };

            var result = await _mediator.Send(query, cancellationToken);
            
            return SuccessResponse(
                result.Data,
                result.PageNumber,
                result.PageSize,
                result.TotalCount,
                "Employees retrieved successfully");
        }

        /// <summary>
        /// Get employee by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetEmployeeById(int id, CancellationToken cancellationToken = default)
        {
            var query = new GetEmployeeByIdQuery { EmployeeId = id };
            var result = await _mediator.Send(query, cancellationToken);
            
            return SuccessResponse(result, "Employee retrieved successfully");
        }

        /// <summary>
        /// Create a new employee
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "ManagerOrAbove")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateEmployee(
            [FromBody] CreateEmployeeCommand command,
            CancellationToken cancellationToken = default)
        {
            command.CreatedBy = GetCurrentUserId();
            var result = await _mediator.Send(command, cancellationToken);
            
            return CreatedAtAction(nameof(GetEmployeeById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Update an existing employee
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "ManagerOrAbove")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateEmployee(
            int id,
            [FromBody] UpdateEmployeeCommand command,
            CancellationToken cancellationToken = default)
        {
            if (id != command.Id)
                return BadRequest("Employee ID mismatch");

            command.ModifiedBy = GetCurrentUserId();
            var result = await _mediator.Send(command, cancellationToken);
            
            return SuccessResponse(result, "Employee updated successfully");
        }

        /// <summary>
        /// Delete an employee
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteEmployee(
            int id,
            CancellationToken cancellationToken = default)
        {
            var command = new DeleteEmployeeCommand { Id = id, DeletedBy = GetCurrentUserId() };
            await _mediator.Send(command, cancellationToken);
            
            return SuccessResponse("Employee deleted successfully");
        }
    }
}
