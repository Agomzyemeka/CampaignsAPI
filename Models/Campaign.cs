using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampaignsAPI.Models
{
    /// <summary>
    /// Represents a marketing campaign entity
    /// Purpose: Core domain model for campaign management system
    /// Interview Notes: 
    /// - Uses data annotations for database constraints
    /// - Audit fields (CreatedAt, UpdatedAt) for tracking
    /// - Indexed fields for query optimization
    /// - Enum for campaign status to ensure data integrity
    /// </summary>
    public class Campaign
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Budget { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public CampaignStatus Status { get; set; } = CampaignStatus.Draft;

        /// <summary>
        /// Foreign key to the user who created the campaign
        /// </summary>
        public int CreatedBy { get; set; }

        /// <summary>
        /// Navigation property to User (for future expansion)
        /// </summary>
        [ForeignKey(nameof(CreatedBy))]
        public virtual User Creator { get; set; } = null!;

        /// <summary>
        /// Audit trail: Timestamp when campaign was created
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Audit trail: Timestamp when campaign was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Soft delete flag - allows logical deletion without losing data
        /// Interview Note: Soft deletes are crucial for maintaining data integrity and audit trails
        /// </summary>
        public bool IsDeleted { get; set; } = false;
    }

    /// <summary>
    /// Campaign lifecycle status enumeration
    /// Purpose: Type-safe status management
    /// Interview Note: Using enums prevents invalid status values at compile-time
    /// </summary>
    public enum CampaignStatus
    {
        Draft = 0,
        Active = 1,
        Paused = 2,
        Completed = 3,
        Cancelled = 4
    }
}
