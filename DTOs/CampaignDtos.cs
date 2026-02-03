using CampaignsAPI.Models;

namespace CampaignsAPI.DTOs
{
    /// <summary>
    /// DTO for creating a new campaign
    /// Purpose: Input validation and data transfer for campaign creation
    /// Interview Note: DTOs separate API contracts from domain models, allowing independent evolution
    /// </summary>
    public class CreateCampaignDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Budget { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public CampaignStatus Status { get; set; } // Using enum for type safety
    }

    /// <summary>
    /// DTO for updating an existing campaign
    /// Purpose: Allows partial updates to campaign data
    /// Interview Note: All fields are nullable to support PATCH-style updates
    /// </summary>
    public class UpdateCampaignDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Budget { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public CampaignStatus? Status { get; set; }
    }

    /// <summary>
    /// DTO for campaign responses
    /// Purpose: Controls exactly what data is exposed via the API
    /// Interview Note: Excludes sensitive data, includes computed fields, formats dates
    /// </summary>
    public class CampaignResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Budget { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty; // Enum converted to string for readability
        public int CreatedBy { get; set; }
        public string CreatorName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        /// <summary>
        /// Computed property: Campaign duration in days
        /// Interview Note: DTOs can include computed/derived fields
        /// </summary>
        public int DurationDays => (EndDate - StartDate).Days;
        
        /// <summary>
        /// Computed property: Is the campaign currently active based on dates
        /// </summary>
        public bool IsCurrentlyActive => DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;
    }

    /// <summary>
    /// Paginated response wrapper
    /// Purpose: Standard pagination pattern for list endpoints
    /// Interview Note: Essential for performance when dealing with large datasets
    /// </summary>
    public class PagedResponse<T>
    {
        public IEnumerable<T> Data { get; set; } = new List<T>();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    /// <summary>
    /// Authentication request DTO
    /// Purpose: User login credentials
    /// </summary>
    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// User registration DTO
    /// Purpose: New user signup
    /// </summary>
    public class RegisterDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Authentication response DTO
    /// Purpose: Returns JWT token and user info after successful authentication
    /// Interview Note: Token-based authentication for stateless REST APIs
    /// </summary>
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    /// <summary>
    /// Standard API response wrapper
    /// Purpose: Consistent response structure across all endpoints
    /// Interview Note: Makes client-side error handling easier
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
