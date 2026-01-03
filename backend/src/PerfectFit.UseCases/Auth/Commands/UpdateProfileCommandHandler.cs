using MediatR;
using PerfectFit.Core.Interfaces;

namespace PerfectFit.UseCases.Auth.Commands;

/// <summary>
/// Handler for UpdateProfileCommand.
/// </summary>
public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UpdateProfileResult>
{
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

        var hasChanges = false;

        // Handle username update
        if (!string.IsNullOrEmpty(request.Username))
        {
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
