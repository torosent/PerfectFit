using PerfectFit.Core.Entities;
using PerfectFit.Core.Interfaces;
using PerfectFit.Core.Services.Results;

namespace PerfectFit.Core.Services;

/// <summary>
/// Service for managing user streaks with timezone-aware calculations.
/// </summary>
public class StreakService : IStreakService
{
    private readonly IUserRepository _userRepository;

    public StreakService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <inheritdoc />
    public async Task<StreakResult> UpdateStreakAsync(User user, DateTimeOffset gameEndTime, CancellationToken ct = default)
    {
        var userTimezone = GetTimezoneInfo(user.Timezone);
        var userLocalTime = TimeZoneInfo.ConvertTime(gameEndTime, userTimezone);
        var playDate = userLocalTime.Date;

        var lastPlayedLocal = user.LastPlayedDate.HasValue
            ? user.LastPlayedDate.Value.Date
            : (DateTime?)null;

        int originalStreak = user.CurrentStreak;
        bool streakBroken = false;
        bool usedFreeze = false;

        if (lastPlayedLocal.HasValue && lastPlayedLocal.Value == playDate)
        {
            // Already played today - no change
            return new StreakResult(
                Success: true,
                NewStreak: user.CurrentStreak,
                LongestStreak: user.LongestStreak,
                StreakBroken: false,
                UsedFreeze: false);
        }

        if (lastPlayedLocal.HasValue)
        {
            var daysSinceLastPlay = (playDate - lastPlayedLocal.Value).Days;

            if (daysSinceLastPlay == 1)
            {
                // Consecutive day - increment streak
                user.UpdateStreak(playDate);
            }
            else if (daysSinceLastPlay == 2 && user.StreakFreezeTokens > 0)
            {
                // Missed exactly one day, but have freeze tokens
                // Use freeze to cover the missed day
                user.UseStreakFreeze();
                usedFreeze = true;
                
                // Simulate playing on the missed day to maintain streak continuity
                var missedDay = lastPlayedLocal.Value.AddDays(1);
                user.UpdateStreak(missedDay);
                
                // Now update for today
                user.UpdateStreak(playDate);
            }
            else
            {
                // Missed too many days - streak broken
                streakBroken = originalStreak > 0;
                user.UpdateStreak(playDate); // This will reset to 1
            }
        }
        else
        {
            // First play ever
            user.UpdateStreak(playDate);
        }

        await _userRepository.UpdateAsync(user, ct);

        return new StreakResult(
            Success: true,
            NewStreak: user.CurrentStreak,
            LongestStreak: user.LongestStreak,
            StreakBroken: streakBroken,
            UsedFreeze: usedFreeze);
    }

    /// <inheritdoc />
    public async Task<bool> UseStreakFreezeAsync(User user, CancellationToken ct = default)
    {
        if (user.StreakFreezeTokens <= 0)
        {
            return false;
        }

        user.UseStreakFreeze();
        await _userRepository.UpdateAsync(user, ct);
        return true;
    }

    /// <inheritdoc />
    public bool IsStreakAtRisk(User user, DateTimeOffset currentTime)
    {
        if (user.CurrentStreak == 0)
        {
            return false;
        }

        var resetTime = GetStreakResetTime(user, currentTime);
        var timeUntilReset = resetTime - currentTime;

        // At risk if less than 1 hour until reset
        return timeUntilReset.TotalHours <= 1 && timeUntilReset.TotalSeconds > 0;
    }

    /// <inheritdoc />
    public DateTimeOffset GetStreakResetTime(User user, DateTimeOffset currentTime)
    {
        var userTimezone = GetTimezoneInfo(user.Timezone);
        var userLocalTime = TimeZoneInfo.ConvertTime(currentTime, userTimezone);

        // Streak resets at midnight in user's timezone
        var nextMidnight = userLocalTime.Date.AddDays(1);
        var nextMidnightUtc = TimeZoneInfo.ConvertTimeToUtc(nextMidnight, userTimezone);

        return new DateTimeOffset(nextMidnightUtc, TimeSpan.Zero);
    }

    private static TimeZoneInfo GetTimezoneInfo(string? timezone)
    {
        if (string.IsNullOrEmpty(timezone))
        {
            return TimeZoneInfo.Utc;
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timezone);
        }
        catch
        {
            return TimeZoneInfo.Utc;
        }
    }
}
