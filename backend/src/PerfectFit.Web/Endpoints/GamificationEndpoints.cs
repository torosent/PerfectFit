using MediatR;
using PerfectFit.Core.Enums;
using PerfectFit.UseCases.Gamification;
using PerfectFit.UseCases.Gamification.Commands;
using PerfectFit.UseCases.Gamification.Queries;
using PerfectFit.Web.DTOs;
using System.Security.Claims;

namespace PerfectFit.Web.Endpoints;

/// <summary>
/// Minimal API endpoints for gamification features.
/// </summary>
public static class GamificationEndpoints
{
    /// <summary>
    /// Maps all gamification-related endpoints.
    /// </summary>
    public static void MapGamificationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/gamification")
            .WithTags("Gamification")
            .RequireAuthorization();

        // GET /api/gamification - Full gamification status
        group.MapGet("/", GetGamificationStatus)
            .WithName("GetGamificationStatus")
            .WithSummary("Get full gamification status for the current user")
            .Produces<GamificationStatusDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        // GET /api/gamification/challenges - Active challenges
        group.MapGet("/challenges", GetChallenges)
            .WithName("GetChallenges")
            .WithSummary("Get active challenges with user progress")
            .Produces<List<ChallengeDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        // GET /api/gamification/achievements - All achievements
        group.MapGet("/achievements", GetAchievements)
            .WithName("GetAchievements")
            .WithSummary("Get all achievements with unlock status")
            .Produces<AchievementsDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        // GET /api/gamification/season-pass - Season pass info
        group.MapGet("/season-pass", GetSeasonPass)
            .WithName("GetSeasonPass")
            .WithSummary("Get current season pass information")
            .Produces<SeasonPassDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        // GET /api/gamification/cosmetics - All cosmetics
        group.MapGet("/cosmetics", GetCosmetics)
            .WithName("GetCosmetics")
            .WithSummary("Get all cosmetics with ownership status")
            .Produces<CosmeticsDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        // GET /api/gamification/goals - Personal goals
        group.MapGet("/goals", GetPersonalGoals)
            .WithName("GetPersonalGoals")
            .WithSummary("Get active personal goals")
            .Produces<List<PersonalGoalDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        // POST /api/gamification/streak-freeze - Use streak freeze
        group.MapPost("/streak-freeze", UseStreakFreeze)
            .WithName("UseStreakFreeze")
            .WithSummary("Use a streak freeze token")
            .Produces<UseStreakFreezeResponseDto>(StatusCodes.Status200OK)
            .Produces<UseStreakFreezeResponseDto>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireRateLimiting("GamificationActionLimit");

        // POST /api/gamification/equip - Equip cosmetic
        group.MapPost("/equip", EquipCosmetic)
            .WithName("EquipCosmetic")
            .WithSummary("Equip a cosmetic item")
            .Produces<EquipCosmeticResponseDto>(StatusCodes.Status200OK)
            .Produces<EquipCosmeticResponseDto>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireRateLimiting("GamificationActionLimit");

        // POST /api/gamification/claim-reward - Claim season reward
        group.MapPost("/claim-reward", ClaimReward)
            .WithName("ClaimReward")
            .WithSummary("Claim a season pass reward")
            .Produces<ClaimRewardResponseDto>(StatusCodes.Status200OK)
            .Produces<ClaimRewardResponseDto>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireRateLimiting("GamificationActionLimit");

        // POST /api/gamification/timezone - Set timezone
        group.MapPost("/timezone", SetTimezone)
            .WithName("SetTimezone")
            .WithSummary("Set user timezone for streak calculations")
            .Produces<SetTimezoneResponseDto>(StatusCodes.Status200OK)
            .Produces<SetTimezoneResponseDto>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireRateLimiting("GamificationActionLimit");
    }

    private static async Task<IResult> GetGamificationStatus(
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = GetUserGuid(user);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var query = new GetGamificationStatusQuery(userId.Value);
        var result = await mediator.Send(query, cancellationToken);

        return Results.Ok(MapToGamificationStatusDto(result));
    }

    private static async Task<IResult> GetChallenges(
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken cancellationToken,
        ChallengeType? type = null)
    {
        var userId = GetUserGuid(user);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var query = new GetChallengesQuery(userId.Value, type);
        var result = await mediator.Send(query, cancellationToken);

        var dtos = result.Select(c => new ChallengeDto(
            Id: c.ChallengeId,
            Name: c.Name,
            Description: c.Description,
            Type: c.Type.ToString(),
            TargetValue: c.TargetValue,
            CurrentProgress: c.CurrentProgress,
            XPReward: c.XPReward,
            IsCompleted: c.IsCompleted,
            EndsAt: c.EndDate
        )).ToList();

        return Results.Ok(dtos);
    }

    private static async Task<IResult> GetAchievements(
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = GetUserGuid(user);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var query = new GetAchievementsQuery(userId.Value);
        var result = await mediator.Send(query, cancellationToken);

        var dto = new AchievementsDto(
            Achievements: result.Achievements.Select(a => new AchievementDto(
                Id: a.AchievementId,
                Name: a.Name,
                Description: a.Description,
                Category: a.Category.ToString(),
                IconUrl: a.IconUrl,
                IsUnlocked: a.IsUnlocked,
                Progress: a.Progress,
                UnlockedAt: a.UnlockedAt,
                IsSecret: a.IsSecret
            )).ToList(),
            TotalUnlocked: result.TotalUnlocked,
            TotalAchievements: result.TotalAchievements
        );

        return Results.Ok(dto);
    }

    private static async Task<IResult> GetSeasonPass(
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = GetUserGuid(user);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var query = new GetSeasonPassQuery(userId.Value);
        var result = await mediator.Send(query, cancellationToken);

        var dto = new SeasonPassDto(
            SeasonPass: result.SeasonPass is null ? null : new SeasonPassInfoDto(
                SeasonId: result.SeasonPass.SeasonId,
                SeasonName: result.SeasonPass.SeasonName,
                SeasonNumber: result.SeasonPass.SeasonNumber,
                CurrentXP: result.SeasonPass.CurrentXP,
                CurrentTier: result.SeasonPass.CurrentTier,
                EndsAt: result.SeasonPass.EndDate,
                Rewards: result.SeasonPass.Rewards.Select(r => new SeasonRewardDto(
                    Id: r.RewardId,
                    Tier: r.Tier,
                    RewardType: r.RewardType.ToString(),
                    RewardValue: r.RewardValue,
                    XPRequired: r.XPRequired,
                    IsClaimed: r.IsClaimed,
                    CanClaim: r.CanClaim
                )).ToList()
            ),
            HasActiveSeason: result.HasActiveSeason
        );

        return Results.Ok(dto);
    }

    private static async Task<IResult> GetCosmetics(
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken cancellationToken,
        CosmeticType? type = null)
    {
        var userId = GetUserGuid(user);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var query = new GetCosmeticsQuery(userId.Value, type);
        var result = await mediator.Send(query, cancellationToken);

        var dto = new CosmeticsDto(
            Cosmetics: result.Cosmetics.Select(c => new CosmeticDto(
                Id: c.CosmeticId,
                Name: c.Name,
                Description: c.Description,
                Type: c.Type.ToString(),
                Rarity: c.Rarity.ToString(),
                AssetUrl: c.AssetUrl,
                PreviewUrl: c.PreviewUrl,
                IsOwned: c.IsOwned,
                IsEquipped: c.IsEquipped,
                IsDefault: c.IsDefault
            )).ToList()
        );

        return Results.Ok(dto);
    }

    private static async Task<IResult> GetPersonalGoals(
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = GetUserGuid(user);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var query = new GetPersonalGoalsQuery(userId.Value);
        var result = await mediator.Send(query, cancellationToken);

        var dtos = result.Select(g => new PersonalGoalDto(
            Id: g.GoalId,
            Type: g.Type.ToString(),
            Description: g.Description,
            TargetValue: g.TargetValue,
            CurrentValue: g.CurrentValue,
            ProgressPercentage: g.ProgressPercentage,
            IsCompleted: g.IsCompleted,
            ExpiresAt: g.ExpiresAt
        )).ToList();

        return Results.Ok(dtos);
    }

    private static async Task<IResult> UseStreakFreeze(
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = GetUserGuid(user);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var command = new UseStreakFreezeCommand(userId.Value);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return Results.BadRequest(new UseStreakFreezeResponseDto(
                Success: false,
                ErrorMessage: result.Error
            ));
        }

        return Results.Ok(new UseStreakFreezeResponseDto(
            Success: result.Value
        ));
    }

    private static async Task<IResult> EquipCosmetic(
        EquipCosmeticRequest request,
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = GetUserGuid(user);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var command = new EquipCosmeticCommand(userId.Value, request.CosmeticId);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return Results.BadRequest(new EquipCosmeticResponseDto(
                Success: false,
                ErrorMessage: result.Error
            ));
        }

        if (!result.Value!.Success)
        {
            return Results.BadRequest(new EquipCosmeticResponseDto(
                Success: false,
                ErrorMessage: result.Value.ErrorMessage
            ));
        }

        return Results.Ok(new EquipCosmeticResponseDto(
            Success: true
        ));
    }

    private static async Task<IResult> ClaimReward(
        ClaimRewardRequest request,
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = GetUserGuid(user);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var command = new ClaimSeasonRewardCommand(userId.Value, request.RewardId);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return Results.BadRequest(new ClaimRewardResponseDto(
                Success: false,
                ErrorMessage: result.Error
            ));
        }

        if (!result.Value!.Success)
        {
            return Results.BadRequest(new ClaimRewardResponseDto(
                Success: false,
                ErrorMessage: result.Value.ErrorMessage
            ));
        }

        return Results.Ok(new ClaimRewardResponseDto(
            Success: true,
            RewardType: result.Value.RewardType?.ToString(),
            RewardValue: result.Value.RewardValue
        ));
    }

    private static async Task<IResult> SetTimezone(
        SetTimezoneRequest request,
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = GetUserGuid(user);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var command = new SetTimezoneCommand(userId.Value, request.Timezone);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return Results.BadRequest(new SetTimezoneResponseDto(
                Success: false,
                ErrorMessage: result.Error
            ));
        }

        return Results.Ok(new SetTimezoneResponseDto(
            Success: true
        ));
    }

    #region Helper Methods

    /// <summary>
    /// Gets a user GUID from the claims principal.
    /// Creates a deterministic GUID from the integer user ID that matches
    /// the handler's GetUserIdFromGuid conversion.
    /// </summary>
    private static Guid? GetUserGuid(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        // The handlers use: Math.Abs(BitConverter.ToInt32(bytes, 0) % 1000000) + 1
        // So we encode (userId - 1) to get the correct userId after decoding
        var valueToEncode = userId - 1;
        var bytes = new byte[16];
        BitConverter.GetBytes(valueToEncode).CopyTo(bytes, 0);
        return new Guid(bytes);
    }

    /// <summary>
    /// Maps a GamificationStatusResult to a GamificationStatusDto.
    /// </summary>
    private static GamificationStatusDto MapToGamificationStatusDto(GamificationStatusResult result)
    {
        return new GamificationStatusDto(
            Streak: new StreakDto(
                CurrentStreak: result.Streak.CurrentStreak,
                LongestStreak: result.Streak.LongestStreak,
                FreezeTokens: result.Streak.FreezeTokens,
                IsAtRisk: result.Streak.IsAtRisk,
                ResetTime: result.Streak.ResetTime
            ),
            ActiveChallenges: result.ActiveChallenges.Select(c => new ChallengeDto(
                Id: c.ChallengeId,
                Name: c.Name,
                Description: c.Description,
                Type: c.Type.ToString(),
                TargetValue: c.TargetValue,
                CurrentProgress: c.CurrentProgress,
                XPReward: c.XPReward,
                IsCompleted: c.IsCompleted,
                EndsAt: c.EndDate
            )).ToList(),
            RecentAchievements: result.RecentAchievements.Select(a => new AchievementDto(
                Id: a.AchievementId,
                Name: a.Name,
                Description: a.Description,
                Category: a.Category.ToString(),
                IconUrl: a.IconUrl,
                IsUnlocked: a.IsUnlocked,
                Progress: a.Progress,
                UnlockedAt: a.UnlockedAt,
                IsSecret: a.IsSecret
            )).ToList(),
            SeasonPass: result.SeasonPass is null ? null : new SeasonPassInfoDto(
                SeasonId: result.SeasonPass.SeasonId,
                SeasonName: result.SeasonPass.SeasonName,
                SeasonNumber: result.SeasonPass.SeasonNumber,
                CurrentXP: result.SeasonPass.CurrentXP,
                CurrentTier: result.SeasonPass.CurrentTier,
                EndsAt: result.SeasonPass.EndDate,
                Rewards: result.SeasonPass.Rewards.Select(r => new SeasonRewardDto(
                    Id: r.RewardId,
                    Tier: r.Tier,
                    RewardType: r.RewardType.ToString(),
                    RewardValue: r.RewardValue,
                    XPRequired: r.XPRequired,
                    IsClaimed: r.IsClaimed,
                    CanClaim: r.CanClaim
                )).ToList()
            ),
            EquippedCosmetics: new EquippedCosmeticsDto(
                BoardTheme: result.EquippedCosmetics.BoardTheme is null ? null : new CosmeticInfoDto(
                    Id: result.EquippedCosmetics.BoardTheme.CosmeticId,
                    Name: result.EquippedCosmetics.BoardTheme.Name,
                    Type: result.EquippedCosmetics.BoardTheme.Type.ToString(),
                    AssetUrl: result.EquippedCosmetics.BoardTheme.AssetUrl,
                    Rarity: result.EquippedCosmetics.BoardTheme.Rarity.ToString()
                ),
                AvatarFrame: result.EquippedCosmetics.AvatarFrame is null ? null : new CosmeticInfoDto(
                    Id: result.EquippedCosmetics.AvatarFrame.CosmeticId,
                    Name: result.EquippedCosmetics.AvatarFrame.Name,
                    Type: result.EquippedCosmetics.AvatarFrame.Type.ToString(),
                    AssetUrl: result.EquippedCosmetics.AvatarFrame.AssetUrl,
                    Rarity: result.EquippedCosmetics.AvatarFrame.Rarity.ToString()
                ),
                Badge: result.EquippedCosmetics.Badge is null ? null : new CosmeticInfoDto(
                    Id: result.EquippedCosmetics.Badge.CosmeticId,
                    Name: result.EquippedCosmetics.Badge.Name,
                    Type: result.EquippedCosmetics.Badge.Type.ToString(),
                    AssetUrl: result.EquippedCosmetics.Badge.AssetUrl,
                    Rarity: result.EquippedCosmetics.Badge.Rarity.ToString()
                )
            ),
            ActiveGoals: result.ActiveGoals.Select(g => new PersonalGoalDto(
                Id: g.GoalId,
                Type: g.Type.ToString(),
                Description: g.Description,
                TargetValue: g.TargetValue,
                CurrentValue: g.CurrentValue,
                ProgressPercentage: g.ProgressPercentage,
                IsCompleted: g.IsCompleted,
                ExpiresAt: g.ExpiresAt
            )).ToList()
        );
    }

    #endregion
}
