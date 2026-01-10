using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
using PerfectFit.Web.DTOs;
using System.Security.Claims;
using System.Text.Json;

namespace PerfectFit.Web.Endpoints;

/// <summary>
/// Admin API endpoints for managing gamification content (Achievements, Challenge Templates, Cosmetics).
/// </summary>
public static class AdminGamificationEndpoints
{
    /// <summary>
    /// Maps all admin gamification endpoints.
    /// </summary>
    public static void MapAdminGamificationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/admin/gamification")
            .WithTags("Admin Gamification")
            .RequireAuthorization("AdminPolicy");

        #region Achievement Endpoints

        // GET /api/admin/gamification/achievements - List paginated
        group.MapGet("/achievements", GetAchievements)
            .WithName("AdminGetAchievements")
            .WithSummary("Get all achievements (paginated)")
            .Produces<PaginatedResponse<AdminAchievementDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        // GET /api/admin/gamification/achievements/{id} - Get single
        group.MapGet("/achievements/{id:int}", GetAchievement)
            .WithName("AdminGetAchievement")
            .WithSummary("Get a single achievement by ID")
            .Produces<AdminAchievementDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        // POST /api/admin/gamification/achievements - Create
        group.MapPost("/achievements", CreateAchievement)
            .WithName("AdminCreateAchievement")
            .WithSummary("Create a new achievement")
            .Produces<AdminAchievementDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        // PUT /api/admin/gamification/achievements/{id} - Update
        group.MapPut("/achievements/{id:int}", UpdateAchievement)
            .WithName("AdminUpdateAchievement")
            .WithSummary("Update an existing achievement")
            .Produces<AdminAchievementDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        // DELETE /api/admin/gamification/achievements/{id} - Delete (with in-use check)
        group.MapDelete("/achievements/{id:int}", DeleteAchievement)
            .WithName("AdminDeleteAchievement")
            .WithSummary("Delete an achievement (fails if in use)")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<EntityInUseResponse>(StatusCodes.Status409Conflict);

        #endregion

        #region Challenge Template Endpoints

        // GET /api/admin/gamification/challenge-templates - List paginated
        group.MapGet("/challenge-templates", GetChallengeTemplates)
            .WithName("AdminGetChallengeTemplates")
            .WithSummary("Get all challenge templates (paginated)")
            .Produces<PaginatedResponse<AdminChallengeTemplateDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        // GET /api/admin/gamification/challenge-templates/{id} - Get single
        group.MapGet("/challenge-templates/{id:int}", GetChallengeTemplate)
            .WithName("AdminGetChallengeTemplate")
            .WithSummary("Get a single challenge template by ID")
            .Produces<AdminChallengeTemplateDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        // POST /api/admin/gamification/challenge-templates - Create
        group.MapPost("/challenge-templates", CreateChallengeTemplate)
            .WithName("AdminCreateChallengeTemplate")
            .WithSummary("Create a new challenge template")
            .Produces<AdminChallengeTemplateDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        // PUT /api/admin/gamification/challenge-templates/{id} - Update
        group.MapPut("/challenge-templates/{id:int}", UpdateChallengeTemplate)
            .WithName("AdminUpdateChallengeTemplate")
            .WithSummary("Update an existing challenge template")
            .Produces<AdminChallengeTemplateDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        // DELETE /api/admin/gamification/challenge-templates/{id} - Delete (with in-use check)
        group.MapDelete("/challenge-templates/{id:int}", DeleteChallengeTemplate)
            .WithName("AdminDeleteChallengeTemplate")
            .WithSummary("Delete a challenge template (fails if in use)")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<EntityInUseResponse>(StatusCodes.Status409Conflict);

        // POST /api/admin/gamification/challenge-templates/{id}/activate - Create active challenge from template
        group.MapPost("/challenge-templates/{id:int}/activate", ActivateChallengeFromTemplate)
            .WithName("AdminActivateChallengeFromTemplate")
            .WithSummary("Create an active challenge from a template (for immediate activation)")
            .Produces<ActivateChallengeResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        // POST /api/admin/gamification/challenges/rotate - Trigger challenge rotation
        group.MapPost("/challenges/rotate", TriggerChallengeRotation)
            .WithName("AdminTriggerChallengeRotation")
            .WithSummary("Trigger challenge rotation to create challenges from all active templates")
            .Produces<ChallengeRotationResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        #endregion

        #region Cosmetic Endpoints

        // GET /api/admin/gamification/cosmetics - List paginated
        group.MapGet("/cosmetics", GetCosmetics)
            .WithName("AdminGetCosmetics")
            .WithSummary("Get all cosmetics (paginated)")
            .Produces<PaginatedResponse<AdminCosmeticDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        // GET /api/admin/gamification/cosmetics/{id} - Get single
        group.MapGet("/cosmetics/{id:int}", GetCosmetic)
            .WithName("AdminGetCosmetic")
            .WithSummary("Get a single cosmetic by ID")
            .Produces<AdminCosmeticDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        // POST /api/admin/gamification/cosmetics - Create
        group.MapPost("/cosmetics", CreateCosmetic)
            .WithName("AdminCreateCosmetic")
            .WithSummary("Create a new cosmetic")
            .Produces<AdminCosmeticDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        // PUT /api/admin/gamification/cosmetics/{id} - Update
        group.MapPut("/cosmetics/{id:int}", UpdateCosmetic)
            .WithName("AdminUpdateCosmetic")
            .WithSummary("Update an existing cosmetic")
            .Produces<AdminCosmeticDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        // DELETE /api/admin/gamification/cosmetics/{id} - Delete (with in-use check)
        group.MapDelete("/cosmetics/{id:int}", DeleteCosmetic)
            .WithName("AdminDeleteCosmetic")
            .WithSummary("Delete a cosmetic (fails if in use)")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<EntityInUseResponse>(StatusCodes.Status409Conflict);

        #endregion
    }

    #region Achievement Handlers

    private static async Task<IResult> GetAchievements(
        ClaimsPrincipal user,
        IGamificationRepository repo,
        IAdminAuditRepository auditRepo,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = GetAdminUserId(user);
        if (adminUserId == null)
        {
            return Results.Unauthorized();
        }

        // Validate and clamp pagination parameters
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var (achievements, totalCount) = await repo.GetAchievementsPagedAsync(page, pageSize, cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        // Create audit log
        var auditLog = AdminAuditLog.Create(
            adminUserId.Value,
            AdminAction.ViewAchievements,
            null,
            $"Viewed achievements page {page}"
        );
        await auditRepo.AddAsync(auditLog, cancellationToken);

        var items = achievements.Select(MapToAdminAchievementDto);

        return Results.Ok(new PaginatedResponse<AdminAchievementDto>(
            Items: items,
            Page: page,
            PageSize: pageSize,
            TotalCount: totalCount,
            TotalPages: totalPages
        ));
    }

    private static async Task<IResult> GetAchievement(
        int id,
        ClaimsPrincipal user,
        IGamificationRepository repo,
        IAdminAuditRepository auditRepo,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = GetAdminUserId(user);
        if (adminUserId == null)
        {
            return Results.Unauthorized();
        }

        var achievement = await repo.GetAchievementByIdAsync(id, cancellationToken);
        if (achievement == null)
        {
            return Results.NotFound();
        }

        // Create audit log
        var auditLog = AdminAuditLog.Create(
            adminUserId.Value,
            AdminAction.ViewAchievements,
            null,
            $"Viewed achievement {achievement.Name} (ID: {id})"
        );
        await auditRepo.AddAsync(auditLog, cancellationToken);

        return Results.Ok(MapToAdminAchievementDto(achievement));
    }

    private static async Task<IResult> CreateAchievement(
        CreateAchievementRequest request,
        ClaimsPrincipal user,
        IGamificationRepository repo,
        IAdminAuditRepository auditRepo,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = GetAdminUserId(user);
        if (adminUserId == null)
        {
            return Results.Unauthorized();
        }

        // Validate JSON in UnlockCondition
        if (!IsValidJson(request.UnlockCondition))
        {
            return Results.BadRequest(new { error = "UnlockCondition must be valid JSON." });
        }

        Achievement achievement;
        try
        {
            achievement = Achievement.Create(
                request.Name,
                request.Description,
                request.Category,
                request.IconUrl,
                request.UnlockCondition,
                request.RewardType,
                request.RewardValue,
                request.IsSecret,
                request.DisplayOrder,
                request.RewardCosmeticCode
            );
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }

        await repo.AddAchievementAsync(achievement, cancellationToken);

        // Create audit log
        var auditLog = AdminAuditLog.Create(
            adminUserId.Value,
            AdminAction.CreateAchievement,
            null,
            $"Created achievement {achievement.Name} (ID: {achievement.Id})"
        );
        await auditRepo.AddAsync(auditLog, cancellationToken);

        return Results.Created($"/api/admin/gamification/achievements/{achievement.Id}", MapToAdminAchievementDto(achievement));
    }

    private static async Task<IResult> UpdateAchievement(
        int id,
        UpdateAchievementRequest request,
        ClaimsPrincipal user,
        IGamificationRepository repo,
        IAdminAuditRepository auditRepo,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = GetAdminUserId(user);
        if (adminUserId == null)
        {
            return Results.Unauthorized();
        }

        var achievement = await repo.GetAchievementByIdAsync(id, cancellationToken);
        if (achievement == null)
        {
            return Results.NotFound();
        }

        // Validate JSON in UnlockCondition
        if (!IsValidJson(request.UnlockCondition))
        {
            return Results.BadRequest(new { error = "UnlockCondition must be valid JSON." });
        }

        try
        {
            achievement.Update(
                request.Name,
                request.Description,
                request.Category,
                request.IconUrl,
                request.UnlockCondition,
                request.RewardType,
                request.RewardValue,
                request.IsSecret,
                request.DisplayOrder,
                request.RewardCosmeticCode
            );
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }

        await repo.UpdateAchievementAsync(achievement, cancellationToken);

        // Create audit log
        var auditLog = AdminAuditLog.Create(
            adminUserId.Value,
            AdminAction.UpdateAchievement,
            null,
            $"Updated achievement {achievement.Name} (ID: {id})"
        );
        await auditRepo.AddAsync(auditLog, cancellationToken);

        return Results.Ok(MapToAdminAchievementDto(achievement));
    }

    private static async Task<IResult> DeleteAchievement(
        int id,
        ClaimsPrincipal user,
        IGamificationRepository repo,
        IAdminAuditRepository auditRepo,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = GetAdminUserId(user);
        if (adminUserId == null)
        {
            return Results.Unauthorized();
        }

        var achievement = await repo.GetAchievementByIdAsync(id, cancellationToken);
        if (achievement == null)
        {
            return Results.NotFound();
        }

        // Check if in use
        var isInUse = await repo.IsAchievementInUseAsync(id, cancellationToken);
        if (isInUse)
        {
            // Create audit log for prevented deletion
            var preventedAuditLog = AdminAuditLog.Create(
                adminUserId.Value,
                AdminAction.DeleteAchievement,
                null,
                $"Delete prevented: achievement '{achievement.Name}' (ID: {id}) is in use"
            );
            await auditRepo.AddAsync(preventedAuditLog, cancellationToken);

            return Results.Conflict(new EntityInUseResponse(
                Message: $"Cannot delete achievement '{achievement.Name}' because it has been unlocked by users.",
                EntityType: "Achievement",
                EntityId: id,
                UsageCount: 1, // Simplified - could query for actual count
                UsageDetails: "This achievement has been unlocked by one or more users."
            ));
        }

        await repo.DeleteAchievementAsync(id, cancellationToken);

        // Create audit log
        var auditLog = AdminAuditLog.Create(
            adminUserId.Value,
            AdminAction.DeleteAchievement,
            null,
            $"Deleted achievement {achievement.Name} (ID: {id})"
        );
        await auditRepo.AddAsync(auditLog, cancellationToken);

        return Results.NoContent();
    }

    #endregion

    #region Challenge Template Handlers

    private static async Task<IResult> GetChallengeTemplates(
        ClaimsPrincipal user,
        IGamificationRepository repo,
        IAdminAuditRepository auditRepo,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = GetAdminUserId(user);
        if (adminUserId == null)
        {
            return Results.Unauthorized();
        }

        // Validate and clamp pagination parameters
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var (templates, totalCount) = await repo.GetChallengeTemplatesPagedAsync(page, pageSize, cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        // Create audit log
        var auditLog = AdminAuditLog.Create(
            adminUserId.Value,
            AdminAction.ViewChallengeTemplates,
            null,
            $"Viewed challenge templates page {page}"
        );
        await auditRepo.AddAsync(auditLog, cancellationToken);

        var items = templates.Select(MapToAdminChallengeTemplateDto);

        return Results.Ok(new PaginatedResponse<AdminChallengeTemplateDto>(
            Items: items,
            Page: page,
            PageSize: pageSize,
            TotalCount: totalCount,
            TotalPages: totalPages
        ));
    }

    private static async Task<IResult> GetChallengeTemplate(
        int id,
        ClaimsPrincipal user,
        IGamificationRepository repo,
        IAdminAuditRepository auditRepo,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = GetAdminUserId(user);
        if (adminUserId == null)
        {
            return Results.Unauthorized();
        }

        var template = await repo.GetChallengeTemplateByIdAsync(id, cancellationToken);
        if (template == null)
        {
            return Results.NotFound();
        }

        // Create audit log
        var auditLog = AdminAuditLog.Create(
            adminUserId.Value,
            AdminAction.ViewChallengeTemplates,
            null,
            $"Viewed challenge template {template.Name} (ID: {id})"
        );
        await auditRepo.AddAsync(auditLog, cancellationToken);

        return Results.Ok(MapToAdminChallengeTemplateDto(template));
    }

    private static async Task<IResult> CreateChallengeTemplate(
        CreateChallengeTemplateRequest request,
        ClaimsPrincipal user,
        IGamificationRepository repo,
        IAdminAuditRepository auditRepo,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = GetAdminUserId(user);
        if (adminUserId == null)
        {
            return Results.Unauthorized();
        }

        ChallengeTemplate template;
        try
        {
            template = ChallengeTemplate.Create(
                request.Name,
                request.Description,
                request.Type,
                request.TargetValue,
                request.XPReward
            );
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }

        await repo.AddChallengeTemplateAsync(template, cancellationToken);

        // Create audit log
        var auditLog = AdminAuditLog.Create(
            adminUserId.Value,
            AdminAction.CreateChallengeTemplate,
            null,
            $"Created challenge template {template.Name} (ID: {template.Id})"
        );
        await auditRepo.AddAsync(auditLog, cancellationToken);

        return Results.Created($"/api/admin/gamification/challenge-templates/{template.Id}", MapToAdminChallengeTemplateDto(template));
    }

    private static async Task<IResult> UpdateChallengeTemplate(
        int id,
        UpdateChallengeTemplateRequest request,
        ClaimsPrincipal user,
        IGamificationRepository repo,
        IAdminAuditRepository auditRepo,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = GetAdminUserId(user);
        if (adminUserId == null)
        {
            return Results.Unauthorized();
        }

        var template = await repo.GetChallengeTemplateByIdAsync(id, cancellationToken);
        if (template == null)
        {
            return Results.NotFound();
        }

        try
        {
            template.Update(
                request.Name,
                request.Description,
                request.Type,
                request.TargetValue,
                request.XPReward,
                request.IsActive
            );
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }

        await repo.UpdateChallengeTemplateAsync(template, cancellationToken);

        // Create audit log
        var auditLog = AdminAuditLog.Create(
            adminUserId.Value,
            AdminAction.UpdateChallengeTemplate,
            null,
            $"Updated challenge template {template.Name} (ID: {id})"
        );
        await auditRepo.AddAsync(auditLog, cancellationToken);

        return Results.Ok(MapToAdminChallengeTemplateDto(template));
    }

    private static async Task<IResult> DeleteChallengeTemplate(
        int id,
        ClaimsPrincipal user,
        IGamificationRepository repo,
        IAdminAuditRepository auditRepo,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = GetAdminUserId(user);
        if (adminUserId == null)
        {
            return Results.Unauthorized();
        }

        var template = await repo.GetChallengeTemplateByIdAsync(id, cancellationToken);
        if (template == null)
        {
            return Results.NotFound();
        }

        // Check if in use
        var isInUse = await repo.IsChallengeTemplateInUseAsync(id, cancellationToken);
        if (isInUse)
        {
            // Create audit log for prevented deletion
            var preventedAuditLog = AdminAuditLog.Create(
                adminUserId.Value,
                AdminAction.DeleteChallengeTemplate,
                null,
                $"Delete prevented: challenge template '{template.Name}' (ID: {id}) is in use"
            );
            await auditRepo.AddAsync(preventedAuditLog, cancellationToken);

            return Results.Conflict(new EntityInUseResponse(
                Message: $"Cannot delete challenge template '{template.Name}' because it has active challenges.",
                EntityType: "ChallengeTemplate",
                EntityId: id,
                UsageCount: 1, // Simplified - could query for actual count
                UsageDetails: "This template has generated challenges that are still active or have been completed by users."
            ));
        }

        await repo.DeleteChallengeTemplateAsync(id, cancellationToken);

        // Create audit log
        var auditLog = AdminAuditLog.Create(
            adminUserId.Value,
            AdminAction.DeleteChallengeTemplate,
            null,
            $"Deleted challenge template {template.Name} (ID: {id})"
        );
        await auditRepo.AddAsync(auditLog, cancellationToken);

        return Results.NoContent();
    }

    /// <summary>
    /// Creates an active challenge from a template immediately.
    /// </summary>
    private static async Task<IResult> ActivateChallengeFromTemplate(
        int id,
        ClaimsPrincipal user,
        IGamificationRepository repo,
        IAdminAuditRepository auditRepo,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = GetAdminUserId(user);
        if (adminUserId == null)
        {
            return Results.Unauthorized();
        }

        var template = await repo.GetChallengeTemplateByIdAsync(id, cancellationToken);
        if (template == null)
        {
            return Results.NotFound(new { Message = $"Challenge template {id} not found" });
        }

        if (!template.IsActive)
        {
            return Results.BadRequest(new { Message = "Cannot activate a challenge from an inactive template" });
        }

        // Calculate dates based on challenge type
        var now = DateTime.UtcNow;
        var startDate = now.Date;
        var endDate = template.Type == ChallengeType.Daily
            ? startDate.AddDays(1)
            : startDate.AddDays(7);

        // Check if an active challenge from this template already exists
        var existingChallenges = await repo.GetActiveChallengesAsync(template.Type, cancellationToken);
        var existingFromTemplate = existingChallenges.FirstOrDefault(c => c.ChallengeTemplateId == id);
        if (existingFromTemplate != null)
        {
            return Results.Conflict(new { Message = $"An active challenge from this template already exists (ID: {existingFromTemplate.Id})" });
        }

        // Create the challenge
        var challenge = Challenge.Create(
            template.Name,
            template.Description,
            template.Type,
            template.TargetValue,
            template.XPReward,
            startDate,
            endDate,
            template.Id
        );

        await repo.AddChallengeAsync(challenge, cancellationToken);

        // Create audit log
        var auditLog = AdminAuditLog.Create(
            adminUserId.Value,
            AdminAction.ActivateChallengeTemplate,
            null,
            $"Activated challenge from template '{template.Name}' (TemplateID: {id}, ChallengeID: {challenge.Id})"
        );
        await auditRepo.AddAsync(auditLog, cancellationToken);

        return Results.Created($"/api/admin/gamification/challenges/{challenge.Id}", new ActivateChallengeResponse(
            ChallengeId: challenge.Id,
            TemplateId: template.Id,
            Name: challenge.Name,
            Type: challenge.Type.ToString(),
            StartDate: challenge.StartDate,
            EndDate: challenge.EndDate,
            Message: $"Challenge '{challenge.Name}' activated successfully"
        ));
    }

    /// <summary>
    /// Triggers challenge rotation to create challenges from all active templates.
    /// </summary>
    private static async Task<IResult> TriggerChallengeRotation(
        ClaimsPrincipal user,
        IGamificationRepository repo,
        IAdminAuditRepository auditRepo,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = GetAdminUserId(user);
        if (adminUserId == null)
        {
            return Results.Unauthorized();
        }

        var now = DateTime.UtcNow;
        var createdChallenges = new List<ChallengeInfo>();
        var skippedTemplates = new List<string>();

        // Get all active templates
        var dailyTemplates = await repo.GetChallengeTemplatesAsync(ChallengeType.Daily, cancellationToken);
        var weeklyTemplates = await repo.GetChallengeTemplatesAsync(ChallengeType.Weekly, cancellationToken);

        // Get existing active challenges
        var activeDailyChallenges = await repo.GetActiveChallengesAsync(ChallengeType.Daily, cancellationToken);
        var activeWeeklyChallenges = await repo.GetActiveChallengesAsync(ChallengeType.Weekly, cancellationToken);

        // Process daily templates
        foreach (var template in dailyTemplates.Where(t => t.IsActive))
        {
            if (activeDailyChallenges.Any(c => c.ChallengeTemplateId == template.Id))
            {
                skippedTemplates.Add($"{template.Name} (Daily) - already active");
                continue;
            }

            var startDate = now.Date;
            var endDate = startDate.AddDays(1);

            var challenge = Challenge.Create(
                template.Name,
                template.Description,
                ChallengeType.Daily,
                template.TargetValue,
                template.XPReward,
                startDate,
                endDate,
                template.Id
            );

            await repo.AddChallengeAsync(challenge, cancellationToken);
            createdChallenges.Add(new ChallengeInfo(challenge.Id, template.Name, "Daily"));
        }

        // Process weekly templates
        foreach (var template in weeklyTemplates.Where(t => t.IsActive))
        {
            if (activeWeeklyChallenges.Any(c => c.ChallengeTemplateId == template.Id))
            {
                skippedTemplates.Add($"{template.Name} (Weekly) - already active");
                continue;
            }

            var startDate = now.Date;
            var endDate = startDate.AddDays(7);

            var challenge = Challenge.Create(
                template.Name,
                template.Description,
                ChallengeType.Weekly,
                template.TargetValue,
                template.XPReward,
                startDate,
                endDate,
                template.Id
            );

            await repo.AddChallengeAsync(challenge, cancellationToken);
            createdChallenges.Add(new ChallengeInfo(challenge.Id, template.Name, "Weekly"));
        }

        // Create audit log
        var auditLog = AdminAuditLog.Create(
            adminUserId.Value,
            AdminAction.TriggerChallengeRotation,
            null,
            $"Triggered challenge rotation: created {createdChallenges.Count} challenges, skipped {skippedTemplates.Count} templates"
        );
        await auditRepo.AddAsync(auditLog, cancellationToken);

        return Results.Ok(new ChallengeRotationResponse(
            CreatedChallenges: createdChallenges,
            SkippedTemplates: skippedTemplates,
            Message: $"Challenge rotation complete. Created {createdChallenges.Count} challenges, skipped {skippedTemplates.Count} templates."
        ));
    }

    #endregion

    #region Cosmetic Handlers

    private static async Task<IResult> GetCosmetics(
        ClaimsPrincipal user,
        IGamificationRepository repo,
        IAdminAuditRepository auditRepo,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = GetAdminUserId(user);
        if (adminUserId == null)
        {
            return Results.Unauthorized();
        }

        // Validate and clamp pagination parameters
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var (cosmetics, totalCount) = await repo.GetCosmeticsPagedAsync(page, pageSize, cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        // Create audit log
        var auditLog = AdminAuditLog.Create(
            adminUserId.Value,
            AdminAction.ViewCosmetics,
            null,
            $"Viewed cosmetics page {page}"
        );
        await auditRepo.AddAsync(auditLog, cancellationToken);

        var items = cosmetics.Select(MapToAdminCosmeticDto);

        return Results.Ok(new PaginatedResponse<AdminCosmeticDto>(
            Items: items,
            Page: page,
            PageSize: pageSize,
            TotalCount: totalCount,
            TotalPages: totalPages
        ));
    }

    private static async Task<IResult> GetCosmetic(
        int id,
        ClaimsPrincipal user,
        IGamificationRepository repo,
        IAdminAuditRepository auditRepo,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = GetAdminUserId(user);
        if (adminUserId == null)
        {
            return Results.Unauthorized();
        }

        var cosmetic = await repo.GetCosmeticByIdAsync(id, cancellationToken);
        if (cosmetic == null)
        {
            return Results.NotFound();
        }

        // Create audit log
        var auditLog = AdminAuditLog.Create(
            adminUserId.Value,
            AdminAction.ViewCosmetics,
            null,
            $"Viewed cosmetic {cosmetic.Name} (ID: {id})"
        );
        await auditRepo.AddAsync(auditLog, cancellationToken);

        return Results.Ok(MapToAdminCosmeticDto(cosmetic));
    }

    private static async Task<IResult> CreateCosmetic(
        CreateCosmeticRequest request,
        ClaimsPrincipal user,
        IGamificationRepository repo,
        IAdminAuditRepository auditRepo,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = GetAdminUserId(user);
        if (adminUserId == null)
        {
            return Results.Unauthorized();
        }

        Cosmetic cosmetic;
        try
        {
            cosmetic = Cosmetic.Create(
                request.Code,
                request.Name,
                request.Description,
                request.Type,
                request.AssetUrl,
                request.PreviewUrl,
                request.Rarity,
                request.IsDefault
            );
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }

        await repo.AddCosmeticAsync(cosmetic, cancellationToken);

        // Create audit log
        var auditLog = AdminAuditLog.Create(
            adminUserId.Value,
            AdminAction.CreateCosmetic,
            null,
            $"Created cosmetic {cosmetic.Name} (ID: {cosmetic.Id})"
        );
        await auditRepo.AddAsync(auditLog, cancellationToken);

        return Results.Created($"/api/admin/gamification/cosmetics/{cosmetic.Id}", MapToAdminCosmeticDto(cosmetic));
    }

    private static async Task<IResult> UpdateCosmetic(
        int id,
        UpdateCosmeticRequest request,
        ClaimsPrincipal user,
        IGamificationRepository repo,
        IAdminAuditRepository auditRepo,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = GetAdminUserId(user);
        if (adminUserId == null)
        {
            return Results.Unauthorized();
        }

        var cosmetic = await repo.GetCosmeticByIdAsync(id, cancellationToken);
        if (cosmetic == null)
        {
            return Results.NotFound();
        }

        try
        {
            cosmetic.Update(
                request.Code,
                request.Name,
                request.Description,
                request.Type,
                request.AssetUrl,
                request.PreviewUrl,
                request.Rarity,
                request.IsDefault
            );
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }

        await repo.UpdateCosmeticAsync(cosmetic, cancellationToken);

        // Create audit log
        var auditLog = AdminAuditLog.Create(
            adminUserId.Value,
            AdminAction.UpdateCosmetic,
            null,
            $"Updated cosmetic {cosmetic.Name} (ID: {id})"
        );
        await auditRepo.AddAsync(auditLog, cancellationToken);

        return Results.Ok(MapToAdminCosmeticDto(cosmetic));
    }

    private static async Task<IResult> DeleteCosmetic(
        int id,
        ClaimsPrincipal user,
        IGamificationRepository repo,
        IAdminAuditRepository auditRepo,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = GetAdminUserId(user);
        if (adminUserId == null)
        {
            return Results.Unauthorized();
        }

        var cosmetic = await repo.GetCosmeticByIdAsync(id, cancellationToken);
        if (cosmetic == null)
        {
            return Results.NotFound();
        }

        // Check if in use
        var isInUse = await repo.IsCosmeticInUseAsync(id, cancellationToken);
        if (isInUse)
        {
            // Create audit log for prevented deletion
            var preventedAuditLog = AdminAuditLog.Create(
                adminUserId.Value,
                AdminAction.DeleteCosmetic,
                null,
                $"Delete prevented: cosmetic '{cosmetic.Name}' (ID: {id}) is in use"
            );
            await auditRepo.AddAsync(preventedAuditLog, cancellationToken);

            return Results.Conflict(new EntityInUseResponse(
                Message: $"Cannot delete cosmetic '{cosmetic.Name}' because it is owned by users or referenced by achievements.",
                EntityType: "Cosmetic",
                EntityId: id,
                UsageCount: 1, // Simplified - could query for actual count
                UsageDetails: "This cosmetic has been unlocked or equipped by one or more users, or is used as a reward in an achievement."
            ));
        }

        await repo.DeleteCosmeticAsync(id, cancellationToken);

        // Create audit log
        var auditLog = AdminAuditLog.Create(
            adminUserId.Value,
            AdminAction.DeleteCosmetic,
            null,
            $"Deleted cosmetic {cosmetic.Name} (ID: {id})"
        );
        await auditRepo.AddAsync(auditLog, cancellationToken);

        return Results.NoContent();
    }

    #endregion

    #region Helper Methods

    private static int? GetAdminUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return null;
    }

    private static bool IsValidJson(string jsonString)
    {
        if (string.IsNullOrWhiteSpace(jsonString))
        {
            return false;
        }

        try
        {
            using var doc = JsonDocument.Parse(jsonString);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static AdminAchievementDto MapToAdminAchievementDto(Achievement achievement)
    {
        return new AdminAchievementDto(
            Id: achievement.Id,
            Name: achievement.Name,
            Description: achievement.Description,
            Category: achievement.Category.ToString(),
            IconUrl: achievement.IconUrl,
            UnlockCondition: achievement.UnlockCondition,
            RewardType: achievement.RewardType.ToString(),
            RewardValue: achievement.RewardValue,
            IsSecret: achievement.IsSecret,
            DisplayOrder: achievement.DisplayOrder,
            RewardCosmeticCode: achievement.RewardCosmeticCode
        );
    }

    private static AdminChallengeTemplateDto MapToAdminChallengeTemplateDto(ChallengeTemplate template)
    {
        return new AdminChallengeTemplateDto(
            Id: template.Id,
            Name: template.Name,
            Description: template.Description,
            Type: template.Type.ToString(),
            TargetValue: template.TargetValue,
            XPReward: template.XPReward,
            IsActive: template.IsActive
        );
    }

    private static AdminCosmeticDto MapToAdminCosmeticDto(Cosmetic cosmetic)
    {
        return new AdminCosmeticDto(
            Id: cosmetic.Id,
            Code: cosmetic.Code,
            Name: cosmetic.Name,
            Description: cosmetic.Description,
            Type: cosmetic.Type.ToString(),
            AssetUrl: cosmetic.AssetUrl,
            PreviewUrl: cosmetic.PreviewUrl,
            Rarity: cosmetic.Rarity.ToString(),
            IsDefault: cosmetic.IsDefault
        );
    }

    #endregion
}

#region Response DTOs for Challenge Activation

/// <summary>
/// Response for activating a single challenge from a template.
/// </summary>
public record ActivateChallengeResponse(
    int ChallengeId,
    int TemplateId,
    string Name,
    string Type,
    DateTime StartDate,
    DateTime EndDate,
    string Message
);

/// <summary>
/// Information about a created challenge.
/// </summary>
public record ChallengeInfo(
    int ChallengeId,
    string Name,
    string Type
);

/// <summary>
/// Response for triggering challenge rotation.
/// </summary>
public record ChallengeRotationResponse(
    IReadOnlyList<ChallengeInfo> CreatedChallenges,
    IReadOnlyList<string> SkippedTemplates,
    string Message
);

#endregion
