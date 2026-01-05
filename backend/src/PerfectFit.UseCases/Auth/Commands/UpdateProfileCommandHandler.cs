using MediatR;
using PerfectFit.Core.Interfaces;
using PerfectFit.Core.Services;

namespace PerfectFit.UseCases.Auth.Commands;

/// <summary>
/// Handler for UpdateProfileCommand.
/// </summary>
public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UpdateProfileResult>
{
    private static readonly TimeSpan DisplayNameCooldownPeriod = TimeSpan.FromDays(7);

    private readonly IUserRepository _userRepository;
    private readonly IDisplayNameValidationService _displayNameValidationService;

    public UpdateProfileCommandHandler(
        IUserRepository userRepository,
        IDisplayNameValidationService displayNameValidationService)
    {
        _userRepository = userRepository;
        _displayNameValidationService = displayNameValidationService;
    }

    public async Task<UpdateProfileResult> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        // Get the user
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return UpdateProfileResult.Failed("User not found.");
        }

        // Avatar validation (if avatar provided and not empty)
        if (request.Avatar is not null && !string.IsNullOrEmpty(request.Avatar) && !AvatarValidator.IsValidAvatar(request.Avatar))
        {
            return UpdateProfileResult.Failed("Invalid avatar. Please select from the available options.");
        }

        var hasChanges = false;

        // Handle display name update
        if (!string.IsNullOrEmpty(request.DisplayName))
        {
            // Display name cooldown check (only if user has changed display name before)
            if (user.LastDisplayNameChangeAt.HasValue)
            {
                var timeSinceLastChange = DateTime.UtcNow - user.LastDisplayNameChangeAt.Value;
                if (timeSinceLastChange < DisplayNameCooldownPeriod)
                {
                    var remainingTime = DisplayNameCooldownPeriod - timeSinceLastChange;
                    return UpdateProfileResult.CooldownActive(remainingTime);
                }
            }

            // Validate display name (format + profanity)
            var validationResult = await _displayNameValidationService.ValidateAsync(request.DisplayName, cancellationToken);
            if (!validationResult.IsValid)
            {
                if (validationResult.SuggestedDisplayName is not null)
                {
                    return UpdateProfileResult.FailedWithSuggestion(
                        validationResult.ErrorMessage!,
                        validationResult.SuggestedDisplayName);
                }
                return UpdateProfileResult.Failed(validationResult.ErrorMessage!);
            }

            // Check uniqueness (excluding current user)
            var isTaken = await _userRepository.IsDisplayNameTakenAsync(request.DisplayName, request.UserId, cancellationToken);
            if (isTaken)
            {
                return UpdateProfileResult.Failed("Display name is already taken.");
            }

            user.SetDisplayName(request.DisplayName);
            hasChanges = true;
        }

        // Handle avatar update
        if (request.Avatar is not null)
        {
            // Empty string clears the avatar
            var newAvatar = string.IsNullOrEmpty(request.Avatar) ? null : request.Avatar;
            user.SetAvatar(newAvatar);
            hasChanges = true;
        }

        // Only save if there were changes
        if (hasChanges)
        {
            await _userRepository.UpdateAsync(user, cancellationToken);
        }

        return UpdateProfileResult.Succeeded(new UserProfileDto(user.Id, user.DisplayName, user.Avatar));
    }
}
