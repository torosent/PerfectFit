using PerfectFit.Core.Enums;
using PerfectFit.Core.Services;

namespace PerfectFit.Core.Entities;

public class User
{
    private const int XPPerTier = 1000;

    public int Id { get; private set; }
    public string ExternalId { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public string? Avatar { get; private set; }
    public AuthProvider Provider { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public int HighScore { get; private set; }
    public int GamesPlayed { get; private set; }
    public DateTime? LastDisplayNameChangeAt { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public string? PasswordHash { get; private set; }
    public bool EmailVerified { get; private set; }
    public string? EmailVerificationToken { get; private set; }
    public DateTime? EmailVerificationTokenExpiry { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockoutEnd { get; private set; }

    // Gamification - Streak fields
    public int CurrentStreak { get; private set; }
    public int LongestStreak { get; private set; }
    public int StreakFreezeTokens { get; private set; }
    public DateTime? LastPlayedDate { get; private set; }
    public string? Timezone { get; private set; }

    // Gamification - Season pass fields
    public int SeasonPassXP { get; private set; }
    public int CurrentSeasonTier { get; private set; }

    // Gamification - Achievement tracking fields
    public int TotalWins { get; private set; }
    public int CurrentWinStreak { get; private set; }
    public int PerfectGames { get; private set; }
    public int HighAccuracyGames { get; private set; }
    public int FastGames { get; private set; }
    public int NightGames { get; private set; }

    // Gamification - Cosmetic fields
    public int? EquippedBoardThemeId { get; private set; }
    public int? EquippedAvatarFrameId { get; private set; }
    public int? EquippedBadgeId { get; private set; }

    // Gamification - Notification tracking
    public DateTime? LastStreakNotificationSentAt { get; private set; }

    // Navigation
    public ICollection<GameSession> GameSessions { get; private set; } = new List<GameSession>();
    public ICollection<UserAchievement> UserAchievements { get; private set; } = new List<UserAchievement>();
    public ICollection<UserChallenge> UserChallenges { get; private set; } = new List<UserChallenge>();
    public ICollection<UserCosmetic> UserCosmetics { get; private set; } = new List<UserCosmetic>();
    public ICollection<PersonalGoal> PersonalGoals { get; private set; } = new List<PersonalGoal>();

    // Private constructor for EF Core
    private User() { }

    public static User Create(string externalId, string? email, string displayName, AuthProvider provider, UserRole role = UserRole.User)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(externalId, nameof(externalId));
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName, nameof(displayName));

        return new User
        {
            ExternalId = externalId,
            Email = email,
            DisplayName = displayName,
            Avatar = null,
            Provider = provider,
            CreatedAt = DateTime.UtcNow,
            HighScore = 0,
            GamesPlayed = 0,
            LastLoginAt = null,
            Role = role,
            IsDeleted = false,
            DeletedAt = null
        };
    }

    /// <summary>
    /// Sets the display name directly (use after async validation).
    /// </summary>
    /// <param name="displayName">The new display name to set (must be pre-validated).</param>
    /// <exception cref="ArgumentException">Thrown when display name format is invalid.</exception>
    public void SetDisplayName(string displayName)
    {
        // Only validate format here - profanity check should be done via IDisplayNameValidationService before calling this
        var formatResult = DisplayNameValidator.ValidateFormat(displayName);
        if (!formatResult.IsValid)
        {
            throw new ArgumentException($"Display name validation failed: {formatResult.ErrorMessage}", nameof(displayName));
        }

        DisplayName = displayName;
        LastDisplayNameChangeAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the avatar emoji.
    /// </summary>
    /// <param name="avatar">The avatar emoji or null to clear.</param>
    public void SetAvatar(string? avatar)
    {
        Avatar = avatar;
    }

    public void UpdateHighScore(int score)
    {
        if (score > HighScore)
        {
            HighScore = score;
        }
    }

    public void IncrementGamesPlayed()
    {
        GamesPlayed++;
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the user's role.
    /// </summary>
    /// <param name="role">The role to assign to the user.</param>
    public void SetRole(UserRole role)
    {
        Role = role;
    }

    /// <summary>
    /// Sets the password hash for local authentication.
    /// </summary>
    /// <param name="passwordHash">The hashed password to set.</param>
    public void SetPasswordHash(string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash, nameof(passwordHash));
        PasswordHash = passwordHash;
    }

    /// <summary>
    /// Sets email verification token and expiry.
    /// </summary>
    /// <param name="token">The verification token.</param>
    /// <param name="expiry">The token expiry time.</param>
    public void SetEmailVerificationToken(string token, DateTime expiry)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token, nameof(token));
        EmailVerificationToken = token;
        EmailVerificationTokenExpiry = expiry;
    }

    /// <summary>
    /// Sets the email verification token expiry (for testing purposes).
    /// </summary>
    /// <param name="expiry">The token expiry time.</param>
    public void SetEmailVerificationTokenExpiry(DateTime expiry)
    {
        EmailVerificationTokenExpiry = expiry;
    }

    /// <summary>
    /// Marks the user's email as verified and clears the verification token.
    /// </summary>
    public void MarkEmailAsVerified()
    {
        EmailVerified = true;
        EmailVerificationToken = null;
        EmailVerificationTokenExpiry = null;
    }

    /// <summary>
    /// Increments the failed login attempts counter.
    /// </summary>
    public void IncrementFailedLoginAttempts()
    {
        FailedLoginAttempts++;
    }

    /// <summary>
    /// Resets the failed login attempts counter and clears any lockout.
    /// </summary>
    public void ResetFailedLoginAttempts()
    {
        FailedLoginAttempts = 0;
        LockoutEnd = null;
    }

    /// <summary>
    /// Checks if the user account is currently locked out.
    /// </summary>
    /// <returns>True if the account is locked out and the lockout hasn't expired; otherwise, false.</returns>
    public bool IsLockedOut()
    {
        return LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;
    }

    /// <summary>
    /// Sets a lockout on the user account until the specified time.
    /// </summary>
    /// <param name="until">The UTC time when the lockout expires.</param>
    public void SetLockout(DateTime until)
    {
        LockoutEnd = until;
    }

    #region Gamification Methods

    /// <summary>
    /// Sets the user's timezone for streak calculations.
    /// </summary>
    /// <param name="timezone">The IANA timezone identifier (e.g., "America/New_York").</param>
    public void SetTimezone(string timezone)
    {
        Timezone = timezone;
    }

    /// <summary>
    /// Updates the user's streak based on play activity.
    /// </summary>
    /// <param name="playDate">The date the user played (in their local timezone).</param>
    public void UpdateStreak(DateTime playDate)
    {
        var playDateOnly = playDate.Date;

        // If already played today, don't increment
        if (LastPlayedDate.HasValue && LastPlayedDate.Value == playDateOnly)
        {
            return;
        }

        // Check if this is a consecutive day
        if (LastPlayedDate.HasValue)
        {
            var daysSinceLastPlay = (playDateOnly - LastPlayedDate.Value).Days;

            if (daysSinceLastPlay == 1)
            {
                // Consecutive day - increment streak
                CurrentStreak++;
            }
            else
            {
                // Missed day(s) - reset streak
                CurrentStreak = 1;
            }
        }
        else
        {
            // First play ever
            CurrentStreak = 1;
        }

        // Update longest streak if needed
        if (CurrentStreak > LongestStreak)
        {
            LongestStreak = CurrentStreak;
        }

        LastPlayedDate = playDateOnly;
    }

    /// <summary>
    /// Uses a streak freeze token to maintain the streak.
    /// </summary>
    /// <returns>True if a token was used successfully, false if no tokens available.</returns>
    public bool UseStreakFreeze()
    {
        if (StreakFreezeTokens <= 0)
        {
            return false;
        }

        StreakFreezeTokens--;
        return true;
    }

    /// <summary>
    /// Adds streak freeze tokens to the user's account.
    /// </summary>
    /// <param name="count">The number of tokens to add.</param>
    public void AddStreakFreezeTokens(int count)
    {
        StreakFreezeTokens += count;
    }

    /// <summary>
    /// Adds XP to the user's season pass and updates tier if needed.
    /// </summary>
    /// <param name="xp">The amount of XP to add.</param>
    public void AddSeasonXP(int xp)
    {
        SeasonPassXP += xp;
        CurrentSeasonTier = SeasonPassXP / XPPerTier;
    }

    /// <summary>
    /// Resets the user's season progress (called at start of new season).
    /// </summary>
    public void ResetSeasonProgress()
    {
        SeasonPassXP = 0;
        CurrentSeasonTier = 0;
    }

    /// <summary>
    /// Equips a cosmetic item of the specified type.
    /// </summary>
    /// <param name="type">The type of cosmetic to equip.</param>
    /// <param name="cosmeticId">The ID of the cosmetic, or null to unequip.</param>
    public void EquipCosmetic(CosmeticType type, int? cosmeticId)
    {
        switch (type)
        {
            case CosmeticType.BoardTheme:
                EquippedBoardThemeId = cosmeticId;
                break;
            case CosmeticType.AvatarFrame:
                EquippedAvatarFrameId = cosmeticId;
                break;
            case CosmeticType.Badge:
                EquippedBadgeId = cosmeticId;
                break;
        }
    }

    /// <summary>
    /// Records that a streak expiry notification was sent.
    /// </summary>
    public void RecordStreakNotificationSent()
    {
        LastStreakNotificationSentAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if a streak notification was sent within the specified hours.
    /// </summary>
    /// <param name="hours">Number of hours to check.</param>
    /// <returns>True if a notification was sent within the specified hours; otherwise, false.</returns>
    public bool WasStreakNotificationSentWithinHours(int hours)
    {
        if (!LastStreakNotificationSentAt.HasValue)
        {
            return false;
        }

        return (DateTime.UtcNow - LastStreakNotificationSentAt.Value).TotalHours < hours;
    }

    /// <summary>
    /// Records a game win and updates related achievement tracking.
    /// </summary>
    public void RecordWin()
    {
        TotalWins++;
        CurrentWinStreak++;
    }

    /// <summary>
    /// Records a game loss and resets the win streak.
    /// </summary>
    public void RecordLoss()
    {
        CurrentWinStreak = 0;
    }

    /// <summary>
    /// Records a perfect game (100% accuracy).
    /// </summary>
    public void RecordPerfectGame()
    {
        PerfectGames++;
    }

    /// <summary>
    /// Records a high accuracy game (95%+ accuracy).
    /// </summary>
    public void RecordHighAccuracyGame()
    {
        HighAccuracyGames++;
    }

    /// <summary>
    /// Records a fast game (completed under the time threshold).
    /// </summary>
    public void RecordFastGame()
    {
        FastGames++;
    }

    /// <summary>
    /// Records a game played during night hours (midnight to 4 AM).
    /// </summary>
    public void RecordNightGame()
    {
        NightGames++;
    }

    #endregion
}
