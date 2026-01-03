using MediatR;
using PerfectFit.Core.Interfaces;
using PerfectFit.Core.Services;

namespace PerfectFit.UseCases.Auth.Commands;

/// <summary>
/// Handler for UpdateProfileCommand.
/// </summary>
public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UpdateProfileResult>
{
    private static readonly TimeSpan UsernameCooldownPeriod = TimeSpan.FromDays(7);

    private readonly IUserRepository _userRepository;
    private readonly IUsernameValidationService _usernameValidationService;

    public UpdateProfileCommandHandler(
        IUserRepository userRepository,
        IUsernameValidationService usernameValidationService)
    {
        _userRepository = userRepository;
        _usernameValidationService = usernameValidationService;
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

        // Handle username update
        if (!string.IsNullOrEmpty(request.Username))
        {
            // Username cooldown check (only if user has changed username before)
            if (user.LastUsernameChangeAt.HasValue)
            {
                var timeSinceLastChange = DateTime.UtcNow - user.LastUsernameChangeAt.Value;
                if (timeSinceLastChange < UsernameCooldownPeriod)
                {
                    var remainingTime = UsernameCooldownPeriod - timeSinceLastChange;
                    return UpdateProfileResult.CooldownActive(remainingTime);
                }
            }

            // Validate username (format + profanity)
            var validationResult = await _usernameValidationService.ValidateAsync(request.Username, cancellationToken);
            if (!validationResult.IsValid)
            {
                if (validationResult.SuggestedUsername is not null)
                {
                    return UpdateProfileResult.FailedWithSuggestion(
                        validationResult.ErrorMessage!,
                        validationResult.SuggestedUsername);
                }
                return UpdateProfileResult.Failed(validationResult.ErrorMessage!);
            }

            // Check uniqueness (excluding current user)
            var isTaken = await _userRepository.IsUsernameTakenAsync(request.Username, request.UserId, cancellationToken);
            if (isTaken)
            {
                return UpdateProfileResult.Failed("Username is already taken.");
            }

            user.SetUsername(request.Username);
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

        return UpdateProfileResult.Succeeded(new UserProfileDto(user.Id, user.Username, user.Avatar));
    }
}
