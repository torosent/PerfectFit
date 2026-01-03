using System.Text.RegularExpressions;
using MediatR;
using Microsoft.Extensions.Options;
using PerfectFit.Core.Configuration;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Identity;
using PerfectFit.Core.Interfaces;

namespace PerfectFit.UseCases.Auth.Commands;

/// <summary>
/// Command for registering a new local user.
/// </summary>
public record RegisterCommand(
    string Email,
    string Password,
    string DisplayName
) : IRequest<RegisterResult>;

/// <summary>
/// Result of a registration operation.
/// </summary>
public record RegisterResult(
    bool Success,
    string? Message = null,
    string? ErrorMessage = null
);

/// <summary>
/// Handler for the register command.
/// </summary>
public partial class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailVerificationService _emailVerificationService;
    private readonly AdminSettings _adminSettings;

    // Password validation regex patterns
    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"[A-Z]")]
    private static partial Regex UppercaseRegex();

    [GeneratedRegex(@"[a-z]")]
    private static partial Regex LowercaseRegex();

    [GeneratedRegex(@"[0-9]")]
    private static partial Regex NumberRegex();

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IEmailVerificationService emailVerificationService,
        IOptions<AdminSettings> adminSettings)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _emailVerificationService = emailVerificationService;
        _adminSettings = adminSettings.Value;
    }

    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Validate email format
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return new RegisterResult(false, ErrorMessage: "Email is required.");
        }

        if (!EmailRegex().IsMatch(request.Email))
        {
            return new RegisterResult(false, ErrorMessage: "The email address is invalid.");
        }

        // Validate display name
        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return new RegisterResult(false, ErrorMessage: "Display name is required.");
        }

        // Validate password strength
        var passwordValidation = ValidatePassword(request.Password);
        if (!passwordValidation.IsValid)
        {
            return new RegisterResult(false, ErrorMessage: passwordValidation.ErrorMessage);
        }

        // Check for duplicate email (using email as external ID for local users)
        var existingUser = await _userRepository.GetByExternalIdAsync(
            request.Email,
            AuthProvider.Local,
            cancellationToken);

        if (existingUser is not null)
        {
            return new RegisterResult(false, ErrorMessage: "An account with this email is already registered.");
        }

        // Check if this email should be an admin
        var shouldBeAdmin = _adminSettings.Emails
            .Any(e => e.Equals(request.Email, StringComparison.OrdinalIgnoreCase));

        // Create the user
        var user = User.Create(
            externalId: request.Email, // Use email as external ID for local users
            email: request.Email,
            displayName: request.DisplayName,
            provider: AuthProvider.Local,
            role: shouldBeAdmin ? UserRole.Admin : UserRole.User);

        // Hash password and set it
        var hashedPassword = _passwordHasher.HashPassword(request.Password);
        user.SetPasswordHash(hashedPassword);

        // Set email verification token
        _emailVerificationService.SetVerificationToken(user);

        // Save the user
        await _userRepository.AddAsync(user, cancellationToken);

        return new RegisterResult(
            Success: true,
            Message: "Registration successful. Please check your email to verify your account.");
    }

    private static (bool IsValid, string? ErrorMessage) ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return (false, "Password is required.");
        }

        if (password.Length < 8)
        {
            return (false, "Password must be at least 8 characters long.");
        }

        if (!UppercaseRegex().IsMatch(password))
        {
            return (false, "Password must contain at least one uppercase letter.");
        }

        if (!LowercaseRegex().IsMatch(password))
        {
            return (false, "Password must contain at least one lowercase letter.");
        }

        if (!NumberRegex().IsMatch(password))
        {
            return (false, "Password must contain at least one number.");
        }

        return (true, null);
    }
}
