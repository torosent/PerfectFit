using MediatR;
using PerfectFit.Core.Interfaces;

namespace PerfectFit.UseCases.Gamification.Commands;

/// <summary>
/// Command to set a user's timezone for streak calculations.
/// </summary>
/// <param name="UserId">The user's external ID (GUID).</param>
/// <param name="Timezone">The IANA timezone identifier (e.g., "America/New_York").</param>
public record SetTimezoneCommand(Guid UserId, string Timezone) : IRequest<Result>;

/// <summary>
/// Handler for setting user timezone.
/// </summary>
public class SetTimezoneCommandHandler : IRequestHandler<SetTimezoneCommand, Result>
{
    private readonly IUserRepository _userRepository;

    public SetTimezoneCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result> Handle(SetTimezoneCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Timezone))
        {
            return Result.Failure("Timezone is required");
        }

        // Validate timezone
        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(request.Timezone);
        }
        catch (TimeZoneNotFoundException)
        {
            // Try IANA format with TimeZoneConverter if available
            if (!IsValidIanaTimezone(request.Timezone))
            {
                return Result.Failure($"Invalid timezone: {request.Timezone}");
            }
        }

        var userId = GetUserIdFromGuid(request.UserId);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return Result.Failure($"User {request.UserId} not found");
        }

        user.SetTimezone(request.Timezone);
        await _userRepository.UpdateAsync(user, cancellationToken);

        return Result.Success();
    }

    private static bool IsValidIanaTimezone(string timezone)
    {
        // Common IANA timezone prefixes
        var validPrefixes = new[]
        {
            "Africa/", "America/", "Antarctica/", "Arctic/", "Asia/",
            "Atlantic/", "Australia/", "Europe/", "Indian/", "Pacific/", "UTC"
        };

        return validPrefixes.Any(prefix => timezone.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    private static int GetUserIdFromGuid(Guid guid)
    {
        var bytes = guid.ToByteArray();
        return Math.Abs(BitConverter.ToInt32(bytes, 0) % 1000000) + 1;
    }
}
