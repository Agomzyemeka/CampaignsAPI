using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CampaignsAPI.DTOs;
using CampaignsAPI.Services;
using FluentValidation;

namespace CampaignsAPI.Controllers
{
    /// <summary>
    /// Authentication Controller
    /// Purpose: Handles user authentication and registration
    /// Interview Notes:
    /// - RESTful API design
    /// - JWT-based authentication
    /// - Input validation with FluentValidation
    /// - Async operations for scalability
    /// - Proper HTTP status codes
    /// - API documentation with XML comments
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IValidator<LoginDto> _loginValidator;
        private readonly IValidator<RegisterDto> _registerValidator;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            IValidator<LoginDto> loginValidator,
            IValidator<RegisterDto> registerValidator,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _loginValidator = loginValidator;
            _registerValidator = registerValidator;
            _logger = logger;
        }

        /// <summary>
        /// User login endpoint
        /// </summary>
        /// <param name="loginDto">Login credentials</param>
        /// <returns>JWT token and user information</returns>
        /// <response code="200">Login successful</response>
        /// <response code="400">Invalid input</response>
        /// <response code="401">Invalid credentials</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            // Validate input
            var validationResult = await _loginValidator.ValidateAsync(loginDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                });
            }

            // Authenticate user
            var authResponse = await _authService.LoginAsync(loginDto);
            if (authResponse == null)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid email or password",
                    Errors = new List<string> { "Authentication failed" }
                });
            }

            _logger.LogInformation("User {Email} logged in successfully", loginDto.Email);

            return Ok(new ApiResponse<AuthResponseDto>
            {
                Success = true,
                Message = "Login successful",
                Data = authResponse
            });
        }

        /// <summary>
        /// User registration endpoint
        /// </summary>
        /// <param name="registerDto">Registration details</param>
        /// <returns>JWT token and user information</returns>
        /// <response code="201">Registration successful</response>
        /// <response code="400">Invalid input or user already exists</response>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            // Validate input
            var validationResult = await _registerValidator.ValidateAsync(registerDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                });
            }

            // Register user
            var authResponse = await _authService.RegisterAsync(registerDto);
            if (authResponse == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Registration failed",
                    Errors = new List<string> { "Email or username already exists" }
                });
            }

            _logger.LogInformation("New user registered: {Email}", registerDto.Email);

            return CreatedAtAction(
                nameof(Register),
                new ApiResponse<AuthResponseDto>
                {
                    Success = true,
                    Message = "Registration successful",
                    Data = authResponse
                });
        }

        /// <summary>
        /// Get current authenticated user info
        /// </summary>
        /// <returns>Current user information</returns>
        /// <response code="200">User information retrieved</response>
        /// <response code="401">Not authenticated</response>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetCurrentUser()
        {
            // Extract user claims from JWT token
            var userId = User.FindFirst("userId")?.Value;
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "User information retrieved",
                Data = new
                {
                    UserId = userId,
                    Email = email,
                    Username = username,
                    Role = role
                }
            });
        }
    }
}
