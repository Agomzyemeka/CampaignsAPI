using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CampaignsAPI.Data;
using CampaignsAPI.Models;
using CampaignsAPI.DTOs;
using FluentValidation;
using System.Linq.Expressions;

namespace CampaignsAPI.Controllers
{
    /// <summary>
    /// Campaigns Controller
    /// Purpose: CRUD operations for campaign management
    /// Interview Notes:
    /// - RESTful API design principles
    /// - Async operations for scalability
    /// - AsNoTracking() for read-only queries (performance optimization)
    /// - Pagination for large datasets
    /// - Filtering and sorting capabilities
    /// - Soft delete implementation
    /// - Role-based authorization
    /// - Comprehensive error handling
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All endpoints require authentication
    [Produces("application/json")]
    public class CampaignsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IValidator<CreateCampaignDto> _createValidator;
        private readonly IValidator<UpdateCampaignDto> _updateValidator;
        private readonly ILogger<CampaignsController> _logger;

        public CampaignsController(
            ApplicationDbContext context,
            IValidator<CreateCampaignDto> createValidator,
            IValidator<UpdateCampaignDto> updateValidator,
            ILogger<CampaignsController> logger)
        {
            _context = context;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        /// <summary>
        /// Get all campaigns with pagination, filtering, and sorting
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10, max: 100)</param>
        /// <param name="status">Filter by campaign status</param>
        /// <param name="searchTerm">Search in name and description</param>
        /// <param name="sortBy">Sort field (default: CreatedAt)</param>
        /// <param name="sortOrder">Sort order: asc or desc (default: desc)</param>
        /// <returns>Paginated list of campaigns</returns>
        /// <response code="200">Campaigns retrieved successfully</response>
        /// <response code="401">Not authenticated</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<CampaignResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCampaigns(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] CampaignStatus? status = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string sortBy = "CreatedAt",
            [FromQuery] string sortOrder = "desc")
        {
            try
            {
                // Validate pagination parameters
                pageNumber = Math.Max(1, pageNumber);
                pageSize = Math.Clamp(pageSize, 1, 100); // Max 100 items per page

                // Build query with AsNoTracking for read-only performance
                // Interview Note: AsNoTracking() improves query performance by ~30%
                var query = _context.Campaigns
                    .AsNoTracking()
                    .Include(c => c.Creator)
                    .Where(c => !c.IsDeleted); // Soft delete filter

                // Apply status filter
                if (status.HasValue)
                {
                    query = query.Where(c => c.Status == status.Value);
                }

                // Apply search filter
                // Interview Note: Contains query can use indexes on Name field
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var lowerSearchTerm = searchTerm.ToLower();
                    query = query.Where(c =>
                        c.Name.ToLower().Contains(lowerSearchTerm) ||
                        c.Description.ToLower().Contains(lowerSearchTerm));
                }

                // Apply sorting
                // Interview Note: Dynamic sorting using Expression trees
                query = ApplySorting(query, sortBy, sortOrder);

                // Get total count for pagination
                var totalRecords = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

                // Apply pagination
                var campaigns = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new CampaignResponseDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        Budget = c.Budget,
                        StartDate = c.StartDate,
                        EndDate = c.EndDate,
                        Status = c.Status.ToString(),
                        CreatedBy = c.CreatedBy,
                        CreatorName = c.Creator.FullName,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    })
                    .ToListAsync();

                var response = new PagedResponse<CampaignResponseDto>
                {
                    Data = campaigns,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = totalPages
                };

                _logger.LogInformation("Retrieved {Count} campaigns (Page {PageNumber})", campaigns.Count, pageNumber);

                return Ok(new ApiResponse<PagedResponse<CampaignResponseDto>>
                {
                    Success = true,
                    Message = "Campaigns retrieved successfully",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving campaigns");
                throw;
            }
        }

        /// <summary>
        /// Get campaign by ID
        /// </summary>
        /// <param name="id">Campaign ID</param>
        /// <returns>Campaign details</returns>
        /// <response code="200">Campaign found</response>
        /// <response code="404">Campaign not found</response>
        /// <response code="401">Not authenticated</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<CampaignResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCampaign(int id)
        {
            var campaign = await _context.Campaigns
                .AsNoTracking()
                .Include(c => c.Creator)
                .Where(c => c.Id == id && !c.IsDeleted)
                .Select(c => new CampaignResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Budget = c.Budget,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Status = c.Status.ToString(),
                    CreatedBy = c.CreatedBy,
                    CreatorName = c.Creator.FullName,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (campaign == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Campaign not found",
                    Errors = new List<string> { $"Campaign with ID {id} does not exist" }
                });
            }

            return Ok(new ApiResponse<CampaignResponseDto>
            {
                Success = true,
                Message = "Campaign retrieved successfully",
                Data = campaign
            });
        }

        /// <summary>
        /// Create a new campaign
        /// </summary>
        /// <param name="createDto">Campaign creation data</param>
        /// <returns>Created campaign</returns>
        /// <response code="201">Campaign created successfully</response>
        /// <response code="400">Invalid input</response>
        /// <response code="401">Not authenticated</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CampaignResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateCampaign([FromBody] CreateCampaignDto createDto)
        {
            // Validate input
            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                });
            }

            // Get current user ID from JWT claims
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid user token"
                });
            }

            // Create campaign entity
            var campaign = new Campaign
            {
                Name = createDto.Name,
                Description = createDto.Description,
                Budget = createDto.Budget,
                StartDate = createDto.StartDate,
                EndDate = createDto.EndDate,
                Status = (CampaignStatus)createDto.Status,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Campaigns.Add(campaign);
            await _context.SaveChangesAsync();

            // Fetch the created campaign with creator info
            var createdCampaign = await _context.Campaigns
                .AsNoTracking()
                .Include(c => c.Creator)
                .Where(c => c.Id == campaign.Id)
                .Select(c => new CampaignResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Budget = c.Budget,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Status = c.Status.ToString(),
                    CreatedBy = c.CreatedBy,
                    CreatorName = c.Creator.FullName,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .FirstOrDefaultAsync();

            _logger.LogInformation("Campaign {CampaignId} created by user {UserId}", campaign.Id, userId);

            return CreatedAtAction(
                nameof(GetCampaign),
                new { id = campaign.Id },
                new ApiResponse<CampaignResponseDto>
                {
                    Success = true,
                    Message = "Campaign created successfully",
                    Data = createdCampaign
                });
        }

        /// <summary>
        /// Update an existing campaign
        /// </summary>
        /// <param name="id">Campaign ID</param>
        /// <param name="updateDto">Campaign update data</param>
        /// <returns>Updated campaign</returns>
        /// <response code="200">Campaign updated successfully</response>
        /// <response code="400">Invalid input</response>
        /// <response code="404">Campaign not found</response>
        /// <response code="403">Not authorized to update this campaign</response>
        /// <response code="401">Not authenticated</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<CampaignResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateCampaign(int id, [FromBody] UpdateCampaignDto updateDto)
        {
            // Validate input
            var validationResult = await _updateValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                });
            }

            // Find campaign
            var campaign = await _context.Campaigns
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (campaign == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Campaign not found"
                });
            }

            // Authorization: Only campaign creator or admin can update
            var userIdClaim = User.FindFirst("userId")?.Value;
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            if (campaign.CreatedBy != userId && userRole != "Admin")
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse<object>
                {
                    Success = false,
                    Message = "You are not authorized to update this campaign"
                });
            }

            // Update fields (only if provided)
            if (!string.IsNullOrEmpty(updateDto.Name))
                campaign.Name = updateDto.Name;

            if (updateDto.Description != null)
                campaign.Description = updateDto.Description;

            if (updateDto.Budget.HasValue)
                campaign.Budget = updateDto.Budget.Value;

            if (updateDto.StartDate.HasValue)
                campaign.StartDate = updateDto.StartDate.Value;

            if (updateDto.EndDate.HasValue)
                campaign.EndDate = updateDto.EndDate.Value;

            if (updateDto.Status.HasValue)
                campaign.Status = (CampaignStatus)updateDto.Status.Value;

            campaign.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Fetch updated campaign
            var updatedCampaign = await _context.Campaigns
                .AsNoTracking()
                .Include(c => c.Creator)
                .Where(c => c.Id == id)
                .Select(c => new CampaignResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Budget = c.Budget,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Status = c.Status.ToString(),
                    CreatedBy = c.CreatedBy,
                    CreatorName = c.Creator.FullName,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .FirstOrDefaultAsync();

            _logger.LogInformation("Campaign {CampaignId} updated by user {UserId}", id, userId);

            return Ok(new ApiResponse<CampaignResponseDto>
            {
                Success = true,
                Message = "Campaign updated successfully",
                Data = updatedCampaign
            });
        }

        /// <summary>
        /// Delete a campaign (soft delete)
        /// </summary>
        /// <param name="id">Campaign ID</param>
        /// <returns>Success message</returns>
        /// <response code="200">Campaign deleted successfully</response>
        /// <response code="404">Campaign not found</response>
        /// <response code="403">Not authorized to delete this campaign</response>
        /// <response code="401">Not authenticated</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteCampaign(int id)
        {
            var campaign = await _context.Campaigns
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (campaign == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Campaign not found"
                });
            }

            // Authorization: Only campaign creator or admin can delete
            var userIdClaim = User.FindFirst("userId")?.Value;
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            if (campaign.CreatedBy != userId && userRole != "Admin")
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse<object>
                {
                    Success = false,
                    Message = "You are not authorized to delete this campaign"
                });
            }

            // Soft delete
            // Interview Note: Soft delete preserves data for audit trails
            campaign.IsDeleted = true;
            campaign.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Campaign {CampaignId} deleted by user {UserId}", id, userId);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Campaign deleted successfully"
            });
        }

        /// <summary>
        /// Get campaign statistics
        /// </summary>
        /// <returns>Campaign statistics</returns>
        /// <response code="200">Statistics retrieved successfully</response>
        /// <response code="401">Not authenticated</response>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetStatistics()
        {
            // Interview Note: Demonstrates aggregate queries and LINQ
            var stats = await _context.Campaigns
                .AsNoTracking()
                .Where(c => !c.IsDeleted)
                .GroupBy(c => 1) // Group all campaigns
                .Select(g => new
                {
                    TotalCampaigns = g.Count(),
                    TotalBudget = g.Sum(c => c.Budget),
                    AverageBudget = g.Average(c => c.Budget),
                    ActiveCampaigns = g.Count(c => c.Status == CampaignStatus.Active),
                    DraftCampaigns = g.Count(c => c.Status == CampaignStatus.Draft),
                    CompletedCampaigns = g.Count(c => c.Status == CampaignStatus.Completed),
                    PausedCampaigns = g.Count(c => c.Status == CampaignStatus.Paused),
                    CancelledCampaigns = g.Count(c => c.Status == CampaignStatus.Cancelled)
                })
                .FirstOrDefaultAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Statistics retrieved successfully",
                Data = stats ?? new
                {
                    TotalCampaigns = 0,
                    TotalBudget = 0m,
                    AverageBudget = 0m,
                    ActiveCampaigns = 0,
                    DraftCampaigns = 0,
                    CompletedCampaigns = 0,
                    PausedCampaigns = 0,
                    CancelledCampaigns = 0
                }
            });
        }

        /// <summary>
        /// Apply dynamic sorting to query
        /// Interview Note: Generic sorting method using Expression trees
        /// </summary>
        private IQueryable<Campaign> ApplySorting(IQueryable<Campaign> query, string sortBy, string sortOrder)
        {
            // Default to descending order
            var isDescending = sortOrder.ToLower() == "desc";

            // Build sorting expression
            Expression<Func<Campaign, object>> sortExpression = sortBy.ToLower() switch
            {
                "name" => c => c.Name,
                "budget" => c => c.Budget,
                "startdate" => c => c.StartDate,
                "enddate" => c => c.EndDate,
                "status" => c => c.Status,
                "createdat" => c => c.CreatedAt,
                _ => c => c.CreatedAt // Default sort
            };

            return isDescending
                ? query.OrderByDescending(sortExpression)
                : query.OrderBy(sortExpression);
        }
    }
}
