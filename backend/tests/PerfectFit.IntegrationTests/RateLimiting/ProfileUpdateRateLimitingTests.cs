using FluentAssertions;
using PerfectFit.Web.DTOs;
using System.Net;
using System.Net.Http.Json;

namespace PerfectFit.IntegrationTests.RateLimiting;

/// <summary>
/// Integration tests for rate limiting on the profile update endpoint.
/// The rate limit is configured as 10 requests per minute per authenticated user.
/// Each test creates its own factory instance to ensure database and rate limiter isolation.
/// </summary>
public class ProfileUpdateRateLimitingTests
{
    [Fact]
    public async Task ProfileUpdate_ReturnsOk_WhenUnderRateLimit()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateAuthenticatedClient(userId: 100, displayName: "TestUser", provider: "Guest");
        var request = new UpdateProfileRequest(Username: "validuser", Avatar: "🎮");

        // Act - Single request should succeed
        var response = await client.PutAsJsonAsync("/api/auth/profile", request);

        // Assert
        // Note: BadRequest is expected when user doesn't exist in the database,
        // but we should NOT get 429 Too Many Requests for a single request
        response.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests,
            "a single request should not trigger rate limiting");
    }

    [Fact]
    public async Task ProfileUpdate_Returns429_WhenRateLimitExceeded()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        // Use a unique user ID to avoid rate limit sharing with other tests
        var uniqueUserId = Random.Shared.Next(10000, 99999);
        var client = factory.CreateAuthenticatedClient(userId: uniqueUserId, displayName: "RateLimitTestUser", provider: "Guest");
        var request = new UpdateProfileRequest(Username: "ratelimituser", Avatar: "🚀");

        // Act - Make 11 requests (limit is 10 per minute)
        var responses = new List<HttpResponseMessage>();
        for (int i = 0; i <= 10; i++)
        {
            var response = await client.PutAsJsonAsync("/api/auth/profile", request);
            responses.Add(response);
        }

        // Assert
        // First 10 requests should not be rate limited (may fail for other reasons like user not existing)
        var first10Responses = responses.Take(10).ToList();
        first10Responses.Should().NotContain(r => r.StatusCode == HttpStatusCode.TooManyRequests,
            "first 10 requests should not be rate limited");

        // The 11th request should be rate limited
        var eleventhResponse = responses[10];
        eleventhResponse.StatusCode.Should().Be(HttpStatusCode.TooManyRequests,
            "the 11th request within 1 minute should be rate limited");
    }

    [Fact]
    public async Task ProfileUpdate_RateLimitIsPerUser()
    {
        // Arrange - Create two different authenticated clients for different users
        await using var factory = new CustomWebApplicationFactory();
        var userId1 = Random.Shared.Next(20000, 29999);
        var userId2 = Random.Shared.Next(30000, 39999);
        var client1 = factory.CreateAuthenticatedClient(userId: userId1, displayName: "User1", provider: "Guest");
        var client2 = factory.CreateAuthenticatedClient(userId: userId2, displayName: "User2", provider: "Guest");
        var request = new UpdateProfileRequest(Username: "testuser", Avatar: "🎯");

        // Act - Use up client1's rate limit
        for (int i = 0; i < 10; i++)
        {
            await client1.PutAsJsonAsync("/api/auth/profile", request);
        }

        // Client2 should still be able to make requests
        var client2Response = await client2.PutAsJsonAsync("/api/auth/profile", request);

        // Assert
        client2Response.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests,
            "rate limit should be per-user, so different user should not be affected");
    }
}
