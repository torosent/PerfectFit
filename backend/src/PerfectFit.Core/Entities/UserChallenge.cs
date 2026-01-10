namespace PerfectFit.Core.Entities;

public class UserChallenge
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int ChallengeId { get; private set; }
    public int CurrentProgress { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    // Navigation properties
    public User? User { get; private set; }
    public Challenge? Challenge { get; private set; }

    // Private constructor for EF Core
    private UserChallenge() { }

    public static UserChallenge Create(int userId, int challengeId)
    {
        return new UserChallenge
        {
            UserId = userId,
            ChallengeId = challengeId,
            CurrentProgress = 0,
            IsCompleted = false,
            CompletedAt = null
        };
    }

    public void UpdateProgress(int progress, int targetValue)
    {
        CurrentProgress = progress;
        CheckCompletion(targetValue);
    }

    public void AddProgress(int amount, int targetValue)
    {
        CurrentProgress += amount;
        CheckCompletion(targetValue);
    }

    private void CheckCompletion(int targetValue)
    {
        if (!IsCompleted && CurrentProgress >= targetValue)
        {
            IsCompleted = true;
            CompletedAt = DateTime.UtcNow;
        }
    }
}
