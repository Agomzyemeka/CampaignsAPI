using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampaignsAPI.Models
{
    /// <summary>
    /// User entity for authentication and authorization
    /// Purpose: Manages user accounts and authentication
    /// Interview Notes:
    /// - Passwords are hashed (never stored in plain text)
    /// - Email is unique and indexed for fast lookups
    /// - Role-based access control ready
    /// - Includes audit fields for compliance
    /// </summary>
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Hashed password using BCrypt or similar
        /// Interview Note: Never store plain text passwords!
        /// </summary>
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// User role for authorization (Admin, Manager, User, etc.)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = "User";

        /// <summary>
        /// Flag to enable/disable user accounts
        /// Interview Note: Better than deleting users - maintains referential integrity
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Account creation timestamp
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last login tracking for security audits
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// Navigation property: Campaigns created by this user
        /// Interview Note: One-to-many relationship with Campaign
        /// </summary>
        public virtual ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();
    }
}
