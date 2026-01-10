namespace PerfectFit.Core.Enums;

public enum AdminAction
{
    // User management actions (0-9)
    ViewUsers = 0,
    ViewUser = 1,
    DeleteUser = 2,
    BulkDeleteUsers = 3,

    // Achievement management actions (10-19)
    ViewAchievements = 10,
    CreateAchievement = 11,
    UpdateAchievement = 12,
    DeleteAchievement = 13,

    // Challenge template management actions (20-29)
    ViewChallengeTemplates = 20,
    CreateChallengeTemplate = 21,
    UpdateChallengeTemplate = 22,
    DeleteChallengeTemplate = 23,

    // Cosmetic management actions (30-39)
    ViewCosmetics = 30,
    CreateCosmetic = 31,
    UpdateCosmetic = 32,
    DeleteCosmetic = 33
}
