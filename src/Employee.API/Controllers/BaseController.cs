using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Employee.API.Controllers
{
    /// <summary>
    /// Base controller for all API endpoints with common functionality
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    public abstract class BaseController : ControllerBase
    {
        protected readonly ILogger<BaseController> _logger;

        protected BaseController(ILogger<BaseController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Returns a successful response with data
        /// </summary>
        protected OkObjectResult SuccessResponse<T>(T data, string message = "Success")
        {
            _logger.LogInformation("Returning success response with data");
            return Ok(new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Returns a successful response without data
        /// </summary>
        protected OkObjectResult SuccessResponse(string message = "Success")
        {
            _logger.LogInformation("Returning success response");
            return Ok(new ApiResponse
            {
                Success = true,
                Message = message,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Returns a paginated response
        /// </summary>
        protected OkObjectResult SuccessResponse<T>(
            IEnumerable<T> data,
            int pageNumber,
            int pageSize,
            int totalCount,
            string message = "Success")
        {
            _logger.LogInformation("Returning paginated response with {Count} items", data.Count());
            
            var response = new PaginatedResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Timestamp = DateTime.UtcNow
            };

            HttpContext.Response.Headers.Add("X-Total-Count", totalCount.ToString());
            HttpContext.Response.Headers.Add("X-Page-Number", pageNumber.ToString());
            HttpContext.Response.Headers.Add("X-Page-Size", pageSize.ToString());
            HttpContext.Response.Headers.Add("X-Total-Pages", response.TotalPages.ToString());

            return Ok(response);
        }

        /// <summary>
        /// Gets the current authenticated user ID
        /// </summary>
        protected string GetCurrentUserId()
        {
            return User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value ?? "Unknown";
        }
    }

    /// <summary>
    /// Standard API response wrapper
    /// </summary>
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Generic API response with data
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Paginated API response
    /// </summary>
    public class PaginatedResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public IEnumerable<T> Data { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
