using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Infrastructure.Data;
using PerfectFit.Web.DTOs;
using System.Net;
using System.Net.Http.Json;

namespace PerfectFit.IntegrationTests.Endpoints;

/// <summary>
/// Integration tests for admin API endpoints.
/// Each test creates its own factory instance to ensure database isolation.
/// </summary>
public class AdminEndpointsTests
{
    [Fact]
    public async Task GetUsers_AsAdmin_ShouldReturnPaginatedUsers()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create admin user
        var adminUser = User.Create("admin-ext-1", "admin@test.com", "Admin User", AuthProvider.Google, UserRole.Admin);
        db.Users.Add(adminUser);
        await db.SaveChangesAsync();

        // Create regular users
        for (int i = 0; i < 5; i++)
        {
            var user = User.Create($"user-ext-{i}", $"user{i}@test.com", $"User {i}", AuthProvider.Google);
            db.Users.Add(user);
        }
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(adminUser.Id, adminUser.DisplayName, "google", UserRole.Admin);

        // Act
        var response = await client.GetAsync("/api/admin/users?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<AdminUserDto>>(CustomWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(6); // 1 admin + 5 regular users
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(6);
    }

    [Fact]
    public async Task GetUsers_AsNonAdmin_ShouldReturn403()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create regular user
        var regularUser = User.Create("regular-ext-1", "regular@test.com", "Regular User", AuthProvider.Google);
        db.Users.Add(regularUser);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(regularUser.Id, regularUser.DisplayName, "google", UserRole.User);

        // Act
        var response = await client.GetAsync("/api/admin/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUser_AsAdmin_ShouldReturnUserDetails()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create admin user
        var adminUser = User.Create("admin-ext-2", "admin2@test.com", "Admin User", AuthProvider.Google, UserRole.Admin);
        db.Users.Add(adminUser);
        await db.SaveChangesAsync();

        // Create target user
        var targetUser = User.Create("target-ext-1", "target@test.com", "Target User", AuthProvider.Microsoft);
        targetUser.UpdateHighScore(500);
        targetUser.IncrementGamesPlayed();
        db.Users.Add(targetUser);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(adminUser.Id, adminUser.DisplayName, "google", UserRole.Admin);

        // Act
        var response = await client.GetAsync($"/api/admin/users/{targetUser.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AdminUserDto>(CustomWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Id.Should().Be(targetUser.Id);
        result.Email.Should().Be("target@test.com");
        result.DisplayName.Should().Be("Target User");
        result.Provider.Should().Be("Microsoft");
        result.HighScore.Should().Be(500);
        result.GamesPlayed.Should().Be(1);
    }

    [Fact]
    public async Task GetUser_NotFound_ShouldReturn404()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create admin user
        var adminUser = User.Create("admin-ext-3", "admin3@test.com", "Admin User", AuthProvider.Google, UserRole.Admin);
        db.Users.Add(adminUser);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(adminUser.Id, adminUser.DisplayName, "google", UserRole.Admin);

        // Act
        var response = await client.GetAsync("/api/admin/users/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteUser_AsAdmin_ShouldSoftDeleteUser()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create admin user
        var adminUser = User.Create("admin-ext-4", "admin4@test.com", "Admin User", AuthProvider.Google, UserRole.Admin);
        db.Users.Add(adminUser);
        await db.SaveChangesAsync();

        // Create target user to delete
        var targetUser = User.Create("target-ext-2", "target2@test.com", "Target User", AuthProvider.Google);
        db.Users.Add(targetUser);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(adminUser.Id, adminUser.DisplayName, "google", UserRole.Admin);

        // Act
        var response = await client.DeleteAsync($"/api/admin/users/{targetUser.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify the user is soft-deleted (use IgnoreQueryFilters to get deleted users)
        db.ChangeTracker.Clear();
        var deletedUser = await db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == targetUser.Id);
        deletedUser.Should().NotBeNull();
        deletedUser!.IsDeleted.Should().BeTrue();
        deletedUser.DeletedAt.Should().NotBeNull();

        // Verify audit log was created with correct admin and target user IDs
        var auditLogs = db.AdminAuditLogs.ToList();
        auditLogs.Should().HaveCount(1);
        auditLogs[0].Action.Should().Be(AdminAction.DeleteUser);
        auditLogs[0].AdminUserId.Should().Be(adminUser.Id); // Should be actual admin ID, not 0
        auditLogs[0].TargetUserId.Should().Be(targetUser.Id); // Should be actual target user ID
    }

    [Fact]
    public async Task DeleteUser_SelfDelete_ShouldReturn400()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create admin user
        var adminUser = User.Create("admin-ext-5", "admin5@test.com", "Admin User", AuthProvider.Google, UserRole.Admin);
        db.Users.Add(adminUser);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(adminUser.Id, adminUser.DisplayName, "google", UserRole.Admin);

        // Act - Admin tries to delete themselves
        var response = await client.DeleteAsync($"/api/admin/users/{adminUser.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errorResponse = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>(CustomWebApplicationFactory.JsonOptions);
        errorResponse.Should().NotBeNull();
        errorResponse!["error"].Should().Contain("delete yourself");
    }

    [Fact]
    public async Task BulkDeleteGuests_AsAdmin_ShouldDeleteAllGuestUsers()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create admin user
        var adminUser = User.Create("admin-ext-6", "admin6@test.com", "Admin User", AuthProvider.Google, UserRole.Admin);
        db.Users.Add(adminUser);
        await db.SaveChangesAsync();

        // Create guest users
        for (int i = 0; i < 3; i++)
        {
            var guestUser = User.Create($"guest-{Guid.NewGuid():N}", null, $"Guest_{i}", AuthProvider.Guest);
            db.Users.Add(guestUser);
        }

        // Create regular user (should not be deleted)
        var regularUser = User.Create("regular-ext-2", "regular2@test.com", "Regular User", AuthProvider.Google);
        db.Users.Add(regularUser);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(adminUser.Id, adminUser.DisplayName, "google", UserRole.Admin);

        // Act
        var response = await client.DeleteAsync("/api/admin/users/bulk/guests");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<BulkDeleteResponse>(CustomWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.DeletedCount.Should().Be(3);

        // Verify guest users are soft-deleted (use IgnoreQueryFilters to get deleted users)
        db.ChangeTracker.Clear();
        var guestUsers = db.Users.IgnoreQueryFilters().Where(u => u.Provider == AuthProvider.Guest).ToList();
        guestUsers.Should().AllSatisfy(u => u.IsDeleted.Should().BeTrue());

        // Verify regular user is NOT deleted
        var notDeletedUser = db.Users.First(u => u.Email == "regular2@test.com");
        notDeletedUser.IsDeleted.Should().BeFalse();

        // Verify audit log was created with correct admin user ID
        var auditLogs = db.AdminAuditLogs.ToList();
        auditLogs.Should().HaveCount(1);
        auditLogs[0].Action.Should().Be(AdminAction.BulkDeleteUsers);
        auditLogs[0].AdminUserId.Should().Be(adminUser.Id); // Should be actual admin ID, not 0
        auditLogs[0].TargetUserId.Should().BeNull(); // Bulk delete has no specific target
    }

    [Fact]
    public async Task GetAuditLogs_AsAdmin_ShouldReturnLogs()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create admin user
        var adminUser = User.Create("admin-ext-7", "admin7@test.com", "Admin User", AuthProvider.Google, UserRole.Admin);
        db.Users.Add(adminUser);
        await db.SaveChangesAsync();

        // Create some audit logs using actual user IDs
        var log1 = AdminAuditLog.Create(adminUser.Id, AdminAction.ViewUsers, null, "Viewed all users");
        var log2 = AdminAuditLog.Create(adminUser.Id, AdminAction.DeleteUser, 999, "Deleted user");
        db.AdminAuditLogs.AddRange(log1, log2);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(adminUser.Id, adminUser.DisplayName, "google", UserRole.Admin);

        // Act
        var response = await client.GetAsync("/api/admin/audit-logs?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<AuditLogDto>>(CustomWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetUsers_Unauthenticated_ShouldReturn401()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/admin/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteUser_NotFound_ShouldReturn404()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create admin user
        var adminUser = User.Create("admin-ext-8", "admin8@test.com", "Admin User", AuthProvider.Google, UserRole.Admin);
        db.Users.Add(adminUser);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(adminUser.Id, adminUser.DisplayName, "google", UserRole.Admin);

        // Act
        var response = await client.DeleteAsync("/api/admin/users/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
