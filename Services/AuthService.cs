using Microsoft.EntityFrameworkCore;
using CampaignsAPI.Data;
using CampaignsAPI.Models;
using CampaignsAPI.DTOs;
using BCrypt.Net;

namespace CampaignsAPI.Services
{
    /// <summary>
    /// Authentication Service Interface
    /// Purpose: Defines authentication operations
    /// </summary>
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
        Task<User?> GetUserByIdAsync(int userId);
    }

    /// <summary>
    /// Authentication Service Implementation
    /// Purpose: Handles user authentication and registration
    /// Interview Notes:
    /// - Password hashing with BCrypt (industry standard)
    /// - Async operations for scalability
    /// - Secure password verification
    /// - Returns JWT token on successful auth
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            ApplicationDbContext context,
            ITokenService tokenService,
            ILogger<AuthService> logger)
        {
            _context = context;
            _tokenService = tokenService;
            _logger = logger;
        }

        /// <summary>
        /// Authenticate user and generate JWT token
        /// Interview Notes:
        /// - Email-based authentication
        /// - BCrypt for password verification (secure against rainbow tables)
        /// - Updates last login timestamp
        /// - Returns token and user info
        /// </summary>
        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            try
            {
                // Find user by email
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.IsActive);

                if (user == null)
                {
                    _logger.LogWarning("Login attempt with invalid email: {Email}", loginDto.Email);
                    return null;
                }

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Login attempt with invalid password for user: {UserId}", user.Id);
                    return null;
                }

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                _context.Users.Attach(user);
                _context.Entry(user).Property(x => x.LastLoginAt).IsModified = true;
                await _context.SaveChangesAsync();

                // Generate token
                var token = _tokenService.GenerateToken(user);
                var expiresAt = DateTime.UtcNow.AddMinutes(60); // Match token expiry

                _logger.LogInformation("User {UserId} logged in successfully", user.Id);

                return new AuthResponseDto
                {
                    Token = token,
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    ExpiresAt = expiresAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", loginDto.Email);
                throw;
            }
        }

        /// <summary>
        /// Register new user
        /// Interview Notes:
        /// - Email uniqueness validation
        /// - Password hashing before storage
        /// - Auto-login after registration
        /// - Transaction safety
        /// </summary>
        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // Check if email already exists
                var existingUser = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == registerDto.Email);

                if (existingUser != null)
                {
                    _logger.LogWarning("Registration attempt with existing email: {Email}", registerDto.Email);
                    return null;
                }

                // Check if username already exists
                var existingUsername = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Username == registerDto.Username);

                if (existingUsername != null)
                {
                    _logger.LogWarning("Registration attempt with existing username: {Username}", registerDto.Username);
                    return null;
                }

                // Create new user
                var user = new User
                {
                    Username = registerDto.Username,
                    Email = registerDto.Email,
                    FullName = registerDto.FullName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                    Role = "User", // Default role
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Generate token for auto-login
                var token = _tokenService.GenerateToken(user);
                var expiresAt = DateTime.UtcNow.AddMinutes(60);

                _logger.LogInformation("New user registered: {UserId}", user.Id);

                return new AuthResponseDto
                {
                    Token = token,
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    ExpiresAt = expiresAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email: {Email}", registerDto.Email);
                throw;
            }
        }

        /// <summary>
        /// Get user by ID
        /// Interview Note: Helper method for authorization checks
        /// </summary>
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
        }
    }
}
