using FluentValidation;
using CampaignsAPI.DTOs;

namespace CampaignsAPI.Validators
{
    /// <summary>
    /// Validator for CreateCampaignDto
    /// Purpose: Enforces business rules and data validation at the API boundary
    /// Interview Notes:
    /// - FluentValidation provides cleaner, more maintainable validation
    /// - Validation happens before hitting the database
    /// - Custom error messages improve API usability
    /// - Can include complex business logic validation
    /// </summary>
    public class CreateCampaignValidator : AbstractValidator<CreateCampaignDto>
    {
        public CreateCampaignValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Campaign name is required")
                .MaximumLength(200).WithMessage("Campaign name cannot exceed 200 characters")
                .MinimumLength(3).WithMessage("Campaign name must be at least 3 characters");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

            RuleFor(x => x.Budget)
                .GreaterThan(0).WithMessage("Budget must be greater than 0")
                .LessThanOrEqualTo(10000000).WithMessage("Budget cannot exceed 10,000,000");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date is required")
                .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
                .WithMessage("Start date cannot be in the past");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("End date is required")
                .GreaterThan(x => x.StartDate)
                .WithMessage("End date must be after start date");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid campaign status");
        }
    }

    /// <summary>
    /// Validator for UpdateCampaignDto
    /// Purpose: Validates partial updates to campaigns
    /// Interview Note: More lenient than Create validator since fields are optional
    /// </summary>
    public class UpdateCampaignValidator : AbstractValidator<UpdateCampaignDto>
    {
        public UpdateCampaignValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(200).WithMessage("Campaign name cannot exceed 200 characters")
                .MinimumLength(3).WithMessage("Campaign name must be at least 3 characters")
                .When(x => !string.IsNullOrEmpty(x.Name));

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.Budget)
                .GreaterThan(0).WithMessage("Budget must be greater than 0")
                .LessThanOrEqualTo(10000000).WithMessage("Budget cannot exceed 10,000,000")
                .When(x => x.Budget.HasValue);

            // Don't validate dates being in the future for updates
            // as campaign might already be running
            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate ?? DateTime.MinValue)
                .WithMessage("End date must be after start date")
                .When(x => x.EndDate.HasValue && x.StartDate.HasValue);

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid campaign status")
                .When(x => x.Status.HasValue);
        }
    }

    /// <summary>
    /// Validator for user registration
    /// Purpose: Ensures user data meets security and business requirements
    /// </summary>
    public class RegisterValidator : AbstractValidator<RegisterDto>
    {
        public RegisterValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters")
                .MaximumLength(100).WithMessage("Username cannot exceed 100 characters")
                .Matches("^[a-zA-Z0-9_-]+$")
                .WithMessage("Username can only contain letters, numbers, hyphens, and underscores");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one number")
                .Matches(@"[@$!%*?&#]").WithMessage("Password must contain at least one special character");

            RuleFor(x => x.FullName)
                .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters");
        }
    }

    /// <summary>
    /// Validator for user login
    /// Purpose: Basic validation for login credentials
    /// </summary>
    public class LoginValidator : AbstractValidator<LoginDto>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required");
        }
    }
}
