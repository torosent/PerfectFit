using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authorization;
using PerfectFit.Core.Enums;
using PerfectFit.Infrastructure.Identity;
using PerfectFit.UseCases.Auth.Commands;
using PerfectFit.Web.DTOs;
using System.Security.Claims;

namespace PerfectFit.Web.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        // GET /api/auth/{provider} - Redirect to OAuth provider
        group.MapGet("/{provider}", InitiateOAuth)
            .WithName("InitiateOAuth")
            .WithDescription("Initiates OAuth flow with the specified provider");

        // GET /api/auth/callback/{provider} - Handle OAuth callback
        group.MapGet("/callback/{provider}", HandleOAuthCallback)
            .WithName("HandleOAuthCallback")
            .WithDescription("Handles the OAuth callback from providers");

        // POST /api/auth/refresh - Refresh token
        group.MapPost("/refresh", RefreshToken)
            .WithName("RefreshToken")
            .WithDescription("Refreshes an expired JWT token");

        // GET /api/auth/me - Get current user
        group.MapGet("/me", GetCurrentUser)
            .RequireAuthorization()
            .WithName("GetCurrentUser")
            .WithDescription("Gets the currently authenticated user");

        // POST /api/auth/logout - Logout (client-side guidance)
        group.MapPost("/logout", Logout)
            .WithName("Logout")
            .WithDescription("Logs out the user (token should be discarded client-side)");

        // POST /api/auth/guest - Create guest session
        group.MapPost("/guest", CreateGuestSession)
            .WithName("CreateGuestSession")
            .WithDescription("Creates a guest user session");

        // PUT /api/auth/profile - Update user profile
        group.MapPut("/profile", UpdateProfile)
            .RequireAuthorization()
            .RequireRateLimiting("ProfileUpdateLimit")
            .WithName("UpdateProfile")
            .WithDescription("Updates user profile (username and/or avatar)");

        // DELETE /api/auth/account - Delete user account
        group.MapDelete("/account", DeleteAccount)
            .RequireAuthorization()
            .WithName("DeleteAccount")
            .WithDescription("Permanently deletes the user account and all associated data");

        // POST /api/auth/register - Register a new local user
        group.MapPost("/register", Register)
            .RequireRateLimiting("RegisterRateLimit")
            .WithName("Register")
            .WithDescription("Registers a new user with email and password");

        // POST /api/auth/login - Login with email and password
        group.MapPost("/login", Login)
            .RequireRateLimiting("LoginRateLimit")
            .WithName("Login")
            .WithDescription("Authenticates a user with email and password");

        // POST /api/auth/verify-email - Verify email address
        group.MapPost("/verify-email", VerifyEmail)
            .WithName("VerifyEmail")
            .WithDescription("Verifies a user's email address with a token");
    }

    private static IResult InitiateOAuth(
        string provider,
        HttpContext httpContext,
        string? returnUrl = null)
    {
        var authProvider = ParseProvider(provider);

        if (authProvider is null)
        {
            return Results.BadRequest(new { error = $"Unsupported OAuth provider: {provider}" });
        }

        var scheme = authProvider switch
        {
            AuthProvider.Google => GoogleDefaults.AuthenticationScheme,
            AuthProvider.Microsoft => MicrosoftAccountDefaults.AuthenticationScheme,
            _ => null
        };

        if (scheme is null)
        {
            return Results.BadRequest(new { error = $"OAuth not configured for provider: {provider}" });
        }

        // Set the return URL (frontend callback URL)
        var callbackUrl = $"/api/auth/callback/{provider.ToLowerInvariant()}";
        var frontendReturnUrl = returnUrl ?? httpContext.Request.Headers["Referer"].ToString() ?? "http://localhost:3000";

        var properties = new AuthenticationProperties
        {
            RedirectUri = callbackUrl,
            Items =
            {
                { "returnUrl", frontendReturnUrl },
                { "provider", provider.ToLowerInvariant() }
            }
        };

        return Results.Challenge(properties, [scheme]);
    }

    private static async Task<IResult> HandleOAuthCallback(
        string provider,
        HttpContext httpContext,
        IMediator mediator)
    {
        var authProvider = ParseProvider(provider);

        if (authProvider is null)
        {
            return Results.BadRequest(new { error = $"Unsupported OAuth provider: {provider}" });
        }

        // Get the external login info
        var authenticateResult = await httpContext.AuthenticateAsync();

        if (!authenticateResult.Succeeded || authenticateResult.Principal is null)
        {
            return Results.Unauthorized();
        }

        var claims = authenticateResult.Principal.Claims.ToList();

        // Extract user info from claims
        var externalId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
                   ?? claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value
                   ?? email?.Split('@')[0]
                   ?? "User";

        if (string.IsNullOrEmpty(externalId))
        {
            return Results.BadRequest(new { error = "Could not retrieve user identifier from provider" });
        }

        // Process OAuth login
        var command = new OAuthLoginCommand(
            ExternalId: externalId,
            Email: email,
            DisplayName: name,
            Provider: authProvider.Value
        );

        var result = await mediator.Send(command);

        // Get the return URL from authentication properties
        var returnUrl = authenticateResult.Properties?.Items["returnUrl"] ?? "http://localhost:3000";

        // Redirect to frontend with token
        var redirectUrl = $"{returnUrl}?token={result.Token}";

        return Results.Redirect(redirectUrl);
    }

    private static async Task<IResult> RefreshToken(
        RefreshTokenRequestDto request,
        IMediator mediator)
    {
        var command = new RefreshTokenCommand(request.Token);
        var result = await mediator.Send(command);

        if (result is null)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new LoginResponseDto(
            Token: result.Token,
            User: new UserDto(
                Id: result.User.Id,
                DisplayName: result.User.DisplayName,
                Username: result.User.Username,
                Avatar: result.User.Avatar,
                Email: result.User.Email,
                Provider: result.User.Provider.ToString(),
                HighScore: result.User.HighScore,
                GamesPlayed: result.User.GamesPlayed
            )
        ));
    }

    private static IResult GetCurrentUser(ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var displayName = user.FindFirst(ClaimTypes.Name)?.Value;
        var email = user.FindFirst(ClaimTypes.Email)?.Value;
        var provider = user.FindFirst("provider")?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new
        {
            Id = int.Parse(userId),
            DisplayName = displayName,
            Email = email,
            Provider = provider
        });
    }

    private static IResult Logout()
    {
        // JWT tokens are stateless - client should discard the token
        return Results.Ok(new { message = "Logged out successfully. Please discard your token." });
    }

    private static async Task<IResult> CreateGuestSession(IMediator mediator)
    {
        var guestId = $"guest-{Guid.NewGuid():N}";
        var command = new OAuthLoginCommand(
            ExternalId: guestId,
            Email: null,
            DisplayName: $"Guest_{guestId[6..12].ToUpperInvariant()}",
            Provider: AuthProvider.Guest
        );

        var result = await mediator.Send(command);

        return Results.Ok(new LoginResponseDto(
            Token: result.Token,
            User: new UserDto(
                Id: result.User.Id,
                DisplayName: result.User.DisplayName,
                Username: result.User.Username,
                Avatar: result.User.Avatar,
                Email: result.User.Email,
                Provider: result.User.Provider.ToString(),
                HighScore: result.User.HighScore,
                GamesPlayed: result.User.GamesPlayed
            )
        ));
    }

    private static async Task<IResult> UpdateProfile(
        UpdateProfileRequest request,
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        var command = new UpdateProfileCommand(userId, request.Username, request.Avatar);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.Success)
        {
            return Results.BadRequest(new UpdateProfileResponse(
                Success: false,
                ErrorMessage: result.ErrorMessage,
                SuggestedUsername: result.SuggestedUsername,
                Profile: null));
        }

        return Results.Ok(new UpdateProfileResponse(
            Success: true,
            ErrorMessage: null,
            SuggestedUsername: null,
            Profile: result.UpdatedProfile is null ? null : new UserProfileResponse(
                Id: result.UpdatedProfile.Id,
                Username: result.UpdatedProfile.Username,
                Avatar: result.UpdatedProfile.Avatar)));
    }

    private static async Task<IResult> DeleteAccount(
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        var command = new DeleteAccountCommand(userId);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.Success)
        {
            return Results.BadRequest(new { error = result.ErrorMessage });
        }

        return Results.Ok(new { message = "Account deleted successfully" });
    }

    private static AuthProvider? ParseProvider(string provider)
    {
        return provider.ToLowerInvariant() switch
        {
            "google" => AuthProvider.Google,
            "facebook" => AuthProvider.Facebook,
            "microsoft" => AuthProvider.Microsoft,
            "guest" => AuthProvider.Guest,
            _ => null
        };
    }

    private static async Task<IResult> Register(
        RegisterRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(
            Email: request.Email,
            Password: request.Password,
            DisplayName: request.DisplayName);

        var result = await mediator.Send(command, cancellationToken);

        if (!result.Success)
        {
            return Results.BadRequest(new RegisterResponse(
                Success: false,
                Message: null,
                ErrorMessage: result.ErrorMessage));
        }

        return Results.Ok(new RegisterResponse(
            Success: true,
            Message: result.Message,
            ErrorMessage: null));
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new LocalLoginCommand(
            Email: request.Email,
            Password: request.Password);

        var result = await mediator.Send(command, cancellationToken);

        if (!result.Success)
        {
            return Results.BadRequest(new LocalLoginResponse(
                Success: false,
                Token: null,
                User: null,
                ErrorMessage: result.ErrorMessage,
                LockoutEnd: result.LockoutEnd));
        }

        return Results.Ok(new LocalLoginResponse(
            Success: true,
            Token: result.Token,
            User: result.User is null ? null : new UserDto(
                Id: result.User.Id,
                DisplayName: result.User.DisplayName,
                Username: result.User.Username,
                Avatar: result.User.Avatar,
                Email: result.User.Email,
                Provider: result.User.Provider.ToString(),
                HighScore: result.User.HighScore,
                GamesPlayed: result.User.GamesPlayed),
            ErrorMessage: null));
    }

    private static async Task<IResult> VerifyEmail(
        VerifyEmailRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new VerifyEmailCommand(
            Email: request.Email,
            Token: request.Token);

        var result = await mediator.Send(command, cancellationToken);

        if (!result.Success)
        {
            return Results.BadRequest(new VerifyEmailResponse(
                Success: false,
                Message: null,
                ErrorMessage: result.ErrorMessage));
        }

        return Results.Ok(new VerifyEmailResponse(
            Success: true,
            Message: result.Message,
            ErrorMessage: null));
    }
}
