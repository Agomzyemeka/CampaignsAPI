using Microsoft.EntityFrameworkCore;
using CampaignsAPI.Models;

namespace CampaignsAPI.Data
{
    /// <summary>
    /// Application Database Context
    /// Purpose: Central point for database operations using Entity Framework Core
    /// Interview Notes:
    /// - DbContext manages entity lifecycle and database connections
    /// - Configured with SQLite for lightweight, file-based storage
    /// - Uses Fluent API for complex configurations
    /// - Implements OnModelCreating for schema customization
    /// - Seed data for testing and development
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Campaigns DbSet - represents the Campaigns table
        /// </summary>
        public DbSet<Campaign> Campaigns { get; set; } = null!;

        /// <summary>
        /// Users DbSet - represents the Users table
        /// </summary>
        public DbSet<User> Users { get; set; } = null!;

        /// <summary>
        /// Configure entity relationships, constraints, and indexes
        /// Interview Notes:
        /// - Fluent API provides more control than data annotations
        /// - Index creation for query optimization
        /// - Cascade delete behaviors for referential integrity
        /// - Default values and computed columns
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================================
            // CAMPAIGN ENTITY CONFIGURATION
            // ============================================
            
            modelBuilder.Entity<Campaign>(entity =>
            {
                // Table name (optional - EF Core uses plural by default)
                entity.ToTable("Campaigns");

                // Primary key
                entity.HasKey(e => e.Id);

                // Indexes for query optimization
                // Interview Note: Index on Name for search operations
                entity.HasIndex(e => e.Name)
                    .HasDatabaseName("IX_Campaigns_Name");

                // Interview Note: Index on Status for filtering active/completed campaigns
                entity.HasIndex(e => e.Status)
                    .HasDatabaseName("IX_Campaigns_Status");

                // Interview Note: Index on CreatedBy for user-specific queries
                entity.HasIndex(e => e.CreatedBy)
                    .HasDatabaseName("IX_Campaigns_CreatedBy");

                // Interview Note: Composite index on StartDate and EndDate for date range queries
                entity.HasIndex(e => new { e.StartDate, e.EndDate })
                    .HasDatabaseName("IX_Campaigns_DateRange");

                // Interview Note: Index on IsDeleted for soft delete filtering
                entity.HasIndex(e => e.IsDeleted)
                    .HasDatabaseName("IX_Campaigns_IsDeleted");

                // Relationships
                entity.HasOne(c => c.Creator)
                    .WithMany(u => u.Campaigns)
                    .HasForeignKey(c => c.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

                // Default values
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("datetime('now')");

                entity.Property(e => e.IsDeleted)
                    .HasDefaultValue(false);

                // Precision for decimal
                entity.Property(e => e.Budget)
                    .HasColumnType("decimal(18,2)");
            });

            // ============================================
            // USER ENTITY CONFIGURATION
            // ============================================
            
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");

                entity.HasKey(e => e.Id);

                // Unique constraint on email
                // Interview Note: Ensures email uniqueness at database level
                entity.HasIndex(e => e.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_Users_Email_Unique");

                // Index on username for search
                entity.HasIndex(e => e.Username)
                    .HasDatabaseName("IX_Users_Username");

                // Index on IsActive for filtering active users
                entity.HasIndex(e => e.IsActive)
                    .HasDatabaseName("IX_Users_IsActive");

                // Default values
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("datetime('now')");

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.Role)
                    .HasDefaultValue("User");
            });

            // ============================================
            // SEED DATA
            // ============================================
            // Interview Note: Seed data for testing and demonstration
            
            SeedData(modelBuilder);
        }

        /// <summary>
        /// Seed initial data for development and testing
        /// Interview Note: Demonstrates database initialization strategy
        /// </summary>
        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Users
            // Password: Admin@123
            var adminUser = new User
            {
                Id = 1,
                Username = "admin",
                Email = "admin@campaigns.com",
                FullName = "System Administrator",
                PasswordHash = "$2a$11$3LZtFmqJ8VKhCqXqYQJ3yO.rqtVEVT2hOxO3FxYZCPNvJhb8nHp7y", // Hashed: Admin@123
                Role = "Admin",
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            var demoUser = new User
            {
                Id = 2,
                Username = "demo",
                Email = "demo@campaigns.com",
                FullName = "Demo User",
                PasswordHash = "$2a$11$3LZtFmqJ8VKhCqXqYQJ3yO.rqtVEVT2hOxO3FxYZCPNvJhb8nHp7y", // Hashed: Admin@123
                Role = "User",
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            modelBuilder.Entity<User>().HasData(adminUser, demoUser);

            // Seed Campaigns
            var campaigns = new[]
            {
                new Campaign
                {
                    Id = 1,
                    Name = "Summer Sale 2024",
                    Description = "Major summer promotion campaign with 30% discounts",
                    Budget = 50000.00m,
                    StartDate = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2024, 8, 31, 0, 0, 0, DateTimeKind.Utc),
                    Status = CampaignStatus.Completed,
                    CreatedBy = 1,
                    CreatedAt = new DateTime(2024, 5, 1, 0, 0, 0, DateTimeKind.Utc),
                    IsDeleted = false
                },
                new Campaign
                {
                    Id = 2,
                    Name = "Holiday Season Campaign",
                    Description = "End of year holiday promotions and special offers",
                    Budget = 75000.00m,
                    StartDate = new DateTime(2024, 12, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2024, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    Status = CampaignStatus.Active,
                    CreatedBy = 1,
                    CreatedAt = new DateTime(2024, 11, 1, 0, 0, 0, DateTimeKind.Utc),
                    IsDeleted = false
                },
                new Campaign
                {
                    Id = 3,
                    Name = "New Product Launch",
                    Description = "Launch campaign for new product line",
                    Budget = 100000.00m,
                    StartDate = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2025, 5, 31, 0, 0, 0, DateTimeKind.Utc),
                    Status = CampaignStatus.Draft,
                    CreatedBy = 2,
                    CreatedAt = new DateTime(2024, 12, 15, 0, 0, 0, DateTimeKind.Utc),
                    IsDeleted = false
                },
                new Campaign
                {
                    Id = 4,
                    Name = "Social Media Boost",
                    Description = "Increase social media engagement through targeted ads",
                    Budget = 25000.00m,
                    StartDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2025, 3, 31, 0, 0, 0, DateTimeKind.Utc),
                    Status = CampaignStatus.Paused,
                    CreatedBy = 2,
                    CreatedAt = new DateTime(2024, 12, 20, 0, 0, 0, DateTimeKind.Utc),
                    IsDeleted = false
                },
                new Campaign
                {
                    Id = 5,
                    Name = "Email Marketing Campaign",
                    Description = "Targeted email campaign for customer retention",
                    Budget = 15000.00m,
                    StartDate = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2025, 4, 30, 0, 0, 0, DateTimeKind.Utc),
                    Status = CampaignStatus.Active,
                    CreatedBy = 1,
                    CreatedAt = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                    IsDeleted = false
                }
            };

            modelBuilder.Entity<Campaign>().HasData(campaigns);
        }

        /// <summary>
        /// Override SaveChanges to automatically update audit fields
        /// Interview Note: Demonstrates automatic timestamp management
        /// </summary>
        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        /// <summary>
        /// Override async SaveChanges for the same audit functionality
        /// </summary>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Automatically set UpdatedAt timestamp on modified entities
        /// Interview Note: Centralized audit trail logic
        /// </summary>
        private void UpdateAuditFields()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Campaign && e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                ((Campaign)entry.Entity).UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
