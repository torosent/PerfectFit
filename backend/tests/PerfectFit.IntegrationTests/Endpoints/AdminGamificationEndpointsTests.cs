using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Infrastructure.Data;
using PerfectFit.Web.DTOs;
using System.Net;
using System.Net.Http.Json;

namespace PerfectFit.IntegrationTests.Endpoints;

/// <summary>
/// Integration tests for admin gamification API endpoints.
/// Each test creates its own factory instance to ensure database isolation.
/// </summary>
public class AdminGamificationEndpointsTests
{
    #region Challenge Template GoalType Tests

    [Fact]
    public async Task CreateChallengeTemplate_WithGoalType_ReturnsGoalTypeInResponse()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create admin user
        var adminUser = User.Create("admin-ext-gam-1", "admin-gam@test.com", "Admin User", AuthProvider.Microsoft, UserRole.Admin);
        db.Users.Add(adminUser);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(adminUser.Id, adminUser.DisplayName, "microsoft", UserRole.Admin);

        var request = new CreateChallengeTemplateRequest(
            Name: "Score Challenge",
            Description: "Score 500 points",
            Type: ChallengeType.Daily,
            TargetValue: 500,
            XPReward: 50,
            GoalType: ChallengeGoalType.ScoreTotal
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/admin/gamification/challenge-templates", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<AdminChallengeTemplateDto>(CustomWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Score Challenge");
        result.GoalType.Should().Be(ChallengeGoalType.ScoreTotal);
    }

    [Fact]
    public async Task CreateChallengeTemplate_WithoutGoalType_ReturnsNullGoalType()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create admin user
        var adminUser = User.Create("admin-ext-gam-2", "admin-gam2@test.com", "Admin User", AuthProvider.Microsoft, UserRole.Admin);
        db.Users.Add(adminUser);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(adminUser.Id, adminUser.DisplayName, "microsoft", UserRole.Admin);

        var request = new CreateChallengeTemplateRequest(
            Name: "Generic Challenge",
            Description: "Complete something",
            Type: ChallengeType.Weekly,
            TargetValue: 10,
            XPReward: 100,
            GoalType: null
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/admin/gamification/challenge-templates", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<AdminChallengeTemplateDto>(CustomWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Generic Challenge");
        result.GoalType.Should().BeNull();
    }

    [Fact]
    public async Task UpdateChallengeTemplate_GoalType_UpdatesSuccessfully()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create admin user
        var adminUser = User.Create("admin-ext-gam-3", "admin-gam3@test.com", "Admin User", AuthProvider.Microsoft, UserRole.Admin);
        db.Users.Add(adminUser);

        // Create a challenge template without GoalType
        var template = ChallengeTemplate.Create(
            name: "Original Challenge",
            description: "Original description",
            type: ChallengeType.Daily,
            targetValue: 100,
            xpReward: 25,
            goalType: null
        );
        db.ChallengeTemplates.Add(template);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(adminUser.Id, adminUser.DisplayName, "microsoft", UserRole.Admin);

        var updateRequest = new UpdateChallengeTemplateRequest(
            Name: "Updated Challenge",
            Description: "Updated description",
            Type: ChallengeType.Daily,
            TargetValue: 200,
            XPReward: 50,
            IsActive: true,
            GoalType: ChallengeGoalType.GameCount
        );

        // Act
        var response = await client.PutAsJsonAsync($"/api/admin/gamification/challenge-templates/{template.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AdminChallengeTemplateDto>(CustomWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Challenge");
        result.GoalType.Should().Be(ChallengeGoalType.GameCount);
    }

    [Fact]
    public async Task GetChallengeTemplates_IncludesGoalTypeInResponse()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create admin user
        var adminUser = User.Create("admin-ext-gam-4", "admin-gam4@test.com", "Admin User", AuthProvider.Microsoft, UserRole.Admin);
        db.Users.Add(adminUser);

        // Create challenge templates with different GoalTypes
        var template1 = ChallengeTemplate.Create(
            name: "Score Challenge",
            description: "Score points",
            type: ChallengeType.Daily,
            targetValue: 500,
            xpReward: 50,
            goalType: ChallengeGoalType.ScoreTotal
        );
        var template2 = ChallengeTemplate.Create(
            name: "Games Challenge",
            description: "Play games",
            type: ChallengeType.Weekly,
            targetValue: 10,
            xpReward: 100,
            goalType: ChallengeGoalType.GameCount
        );
        var template3 = ChallengeTemplate.Create(
            name: "No Goal Type",
            description: "Generic challenge",
            type: ChallengeType.Daily,
            targetValue: 5,
            xpReward: 25,
            goalType: null
        );
        db.ChallengeTemplates.AddRange(template1, template2, template3);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(adminUser.Id, adminUser.DisplayName, "microsoft", UserRole.Admin);

        // Act
        var response = await client.GetAsync("/api/admin/gamification/challenge-templates");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<AdminChallengeTemplateDto>>(CustomWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(3);

        var scoreChallenge = result.Items.FirstOrDefault(t => t.Name == "Score Challenge");
        scoreChallenge.Should().NotBeNull();
        scoreChallenge!.GoalType.Should().Be(ChallengeGoalType.ScoreTotal);

        var gamesChallenge = result.Items.FirstOrDefault(t => t.Name == "Games Challenge");
        gamesChallenge.Should().NotBeNull();
        gamesChallenge!.GoalType.Should().Be(ChallengeGoalType.GameCount);

        var noGoalType = result.Items.FirstOrDefault(t => t.Name == "No Goal Type");
        noGoalType.Should().NotBeNull();
        noGoalType!.GoalType.Should().BeNull();
    }

    [Fact]
    public async Task GetChallengeTemplate_SingleById_IncludesGoalType()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create admin user
        var adminUser = User.Create("admin-ext-gam-5", "admin-gam5@test.com", "Admin User", AuthProvider.Microsoft, UserRole.Admin);
        db.Users.Add(adminUser);

        // Create a challenge template with GoalType
        var template = ChallengeTemplate.Create(
            name: "Single Game Score Challenge",
            description: "Beat a score in a single game",
            type: ChallengeType.Daily,
            targetValue: 1,
            xpReward: 75,
            goalType: ChallengeGoalType.ScoreSingleGame
        );
        db.ChallengeTemplates.Add(template);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(adminUser.Id, adminUser.DisplayName, "microsoft", UserRole.Admin);

        // Act
        var response = await client.GetAsync($"/api/admin/gamification/challenge-templates/{template.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AdminChallengeTemplateDto>(CustomWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Single Game Score Challenge");
        result.GoalType.Should().Be(ChallengeGoalType.ScoreSingleGame);
    }

    #endregion
}
