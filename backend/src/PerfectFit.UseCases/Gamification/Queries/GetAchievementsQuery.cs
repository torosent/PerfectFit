using MediatR;
using PerfectFit.Core.Interfaces;

namespace PerfectFit.UseCases.Gamification.Queries;

/// <summary>
/// Query to get all achievements with user's progress and unlock status.
/// </summary>
/// <param name="UserId">The user's external ID (GUID).</param>
public record GetAchievementsQuery(Guid UserId) : IRequest<AchievementsResult>;

/// <summary>
/// Handler for getting achievements.
/// </summary>
public class GetAchievementsQueryHandler : IRequestHandler<GetAchievementsQuery, AchievementsResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IAchievementService _achievementService;

    public GetAchievementsQueryHandler(IUserRepository userRepository, IAchievementService achievementService)
    {
        _userRepository = userRepository;
        _achievementService = achievementService;
    }

    public async Task<AchievementsResult> Handle(GetAchievementsQuery request, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromGuid(request.UserId);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException($"User {request.UserId} not found");
        }

        var allAchievements = await _achievementService.GetAllAchievementsAsync(cancellationToken);
        var userAchievements = await _achievementService.GetUserAchievementsAsync(userId, cancellationToken);

        var userAchievementDict = userAchievements.ToDictionary(ua => ua.AchievementId);

        var achievementInfos = allAchievements.Select(a =>
        {
            var userAchievement = userAchievementDict.GetValueOrDefault(a.Id);
            return new AchievementInfo(
                AchievementId: a.Id,
                Name: a.Name,
                Description: a.Description,
                Category: a.Category,
                IconUrl: a.IconUrl,
                Progress: userAchievement?.Progress ?? 0,
                IsUnlocked: userAchievement?.IsUnlocked ?? false,
                UnlockedAt: userAchievement?.UnlockedAt,
                IsSecret: a.IsSecret
            );
        }).ToList();

        var totalUnlocked = achievementInfos.Count(a => a.IsUnlocked);

        return new AchievementsResult(
            Achievements: achievementInfos,
            TotalUnlocked: totalUnlocked,
            TotalAchievements: achievementInfos.Count
        );
    }

    private static int GetUserIdFromGuid(Guid guid)
    {
        var bytes = guid.ToByteArray();
        return Math.Abs(BitConverter.ToInt32(bytes, 0) % 1000000) + 1;
    }
}
