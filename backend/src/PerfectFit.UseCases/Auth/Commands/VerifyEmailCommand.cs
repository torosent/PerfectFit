using MediatR;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Identity;
using PerfectFit.Core.Interfaces;

namespace PerfectFit.UseCases.Auth.Commands;

/// <summary>
/// Command for verifying a user's email address.
/// </summary>
public record VerifyEmailCommand(
    string Email,
    string Token
) : IRequest<VerifyEmailResult>;

/// <summary>
/// Result of an email verification operation.
/// </summary>
public record VerifyEmailResult(
    bool Success,
    string? Message = null,
    string? ErrorMessage = null
);

/// <summary>
/// Handler for the verify email command.
/// </summary>
public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, VerifyEmailResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailVerificationService _emailVerificationService;

    public VerifyEmailCommandHandler(
        IUserRepository userRepository,
        IEmailVerificationService emailVerificationService)
    {
        _userRepository = userRepository;
        _emailVerificationService = emailVerificationService;
    }

    public async Task<VerifyEmailResult> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return new VerifyEmailResult(false, ErrorMessage: "Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return new VerifyEmailResult(false, ErrorMessage: "Verification token is required.");
        }

        // Find user by email (local users use email as external ID)
        var user = await _userRepository.GetByExternalIdAsync(
            request.Email,
            AuthProvider.Local,
            cancellationToken);

        if (user is null)
        {
            return new VerifyEmailResult(false, ErrorMessage: "Invalid email or verification token.");
        }

        // Check if already verified
        if (user.EmailVerified)
        {
            return new VerifyEmailResult(true, Message: "Email address is already verified.");
        }

        // Validate token
        if (!_emailVerificationService.IsTokenValid(user, request.Token))
        {
            return new VerifyEmailResult(false, ErrorMessage: "Invalid or expired verification token.");
        }

        // Mark email as verified
        _emailVerificationService.MarkEmailVerified(user);
        await _userRepository.UpdateAsync(user, cancellationToken);

        return new VerifyEmailResult(true, Message: "Email address has been verified successfully.");
    }
}
