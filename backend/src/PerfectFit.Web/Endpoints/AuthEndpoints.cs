using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PerfectFit.Core.Enums;
using PerfectFit.Core.Interfaces;
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
            .RequireRateLimiting("GuestRateLimit")
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
        IConfiguration configuration,
        string? redirect = null,
        string? returnUrl = null)
    {
        var authProvider = ParseProvider(provider);

        if (authProvider is null)
        {
            return Results.BadRequest(new { error = $"Unsupported OAuth provider: {provider}" });
        }

        // Only Microsoft OAuth is supported
        if (authProvider != AuthProvider.Microsoft)
        {
            return Results.BadRequest(new { error = $"OAuth provider '{provider}' is not supported. Only Microsoft OAuth is available." });
        }

        var scheme = MicrosoftAccountDefaults.AuthenticationScheme;

        // Set the return URL (frontend callback URL)
        var callbackUrl = $"/api/auth/callback/{provider.ToLowerInvariant()}";
        var requestedRedirectUrl = redirect ?? returnUrl;
        var frontendReturnUrl = ResolveRedirectUrl(requestedRedirectUrl, configuration);

        var properties = new AuthenticationProperties
        {
            RedirectUri = callbackUrl,
            Items =
            {
                { "returnUrl", frontendReturnUrl },
                { "provider", provider.ToLowerInvariant() }
            }
        };

        return Results.Challenge(properties, new[] { scheme });
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

        // Get the external login info using the cookie scheme
        var authenticateResult = await httpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

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

        var frontendReturnUrl = authenticateResult.Properties?.Items["returnUrl"]
            ?? ResolveRedirectUrl(null, httpContext.RequestServices.GetRequiredService<IConfiguration>());

        var redirectUrl = BuildCallbackRedirectUrl(frontendReturnUrl, result.Token);

        return Results.Redirect(redirectUrl);
    }

    private static async Task<IResult> RefreshToken(
        RefreshTokenRequestDto? request,
        HttpContext httpContext,
        IMediator mediator)
    {
        var token = request?.Token;

        if (string.IsNullOrWhiteSpace(token))
        {
            var authorization = httpContext.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrWhiteSpace(authorization) && authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authorization.Substring("Bearer ".Length).Trim();
            }
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            token = httpContext.Request.Cookies["pf_auth"];
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return Results.BadRequest(new { error = "Refresh token is required." });
        }

        var command = new RefreshTokenCommand(token);
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
                Avatar: result.User.Avatar,
                Email: result.User.Email,
                Provider: result.User.Provider.ToString().ToLowerInvariant(),
                HighScore: result.User.HighScore,
                GamesPlayed: result.User.GamesPlayed
            )
        ));
    }

    private static async Task<IResult> GetCurrentUser(
        ClaimsPrincipal user,
        IUserRepository userRepository)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        var dbUser = await userRepository.GetByIdAsync(userId);
        if (dbUser == null)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new UserDto(
            Id: dbUser.Id,
            DisplayName: dbUser.DisplayName,
            Avatar: dbUser.Avatar,
            Email: dbUser.Email,
            Provider: dbUser.Provider.ToString().ToLowerInvariant(),
            HighScore: dbUser.HighScore,
            GamesPlayed: dbUser.GamesPlayed,
            Role: dbUser.Role.ToString()
        ));
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
                Avatar: result.User.Avatar,
                Email: result.User.Email,
                Provider: result.User.Provider.ToString().ToLowerInvariant(),
                HighScore: result.User.HighScore,
                GamesPlayed: result.User.GamesPlayed,
                Role: result.User.Role.ToString()
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

        var command = new UpdateProfileCommand(userId, request.DisplayName, request.Avatar);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.Success)
        {
            return Results.BadRequest(new UpdateProfileResponse(
                Success: false,
                ErrorMessage: result.ErrorMessage,
                SuggestedDisplayName: result.SuggestedDisplayName,
                Profile: null));
        }

        return Results.Ok(new UpdateProfileResponse(
            Success: true,
            ErrorMessage: null,
            SuggestedDisplayName: null,
            Profile: result.UpdatedProfile is null ? null : new UserProfileResponse(
                Id: result.UpdatedProfile.Id,
                DisplayName: result.UpdatedProfile.DisplayName,
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

    private static string ResolveRedirectUrl(string? requestedRedirectUrl, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        var defaultFrontendUrl = configuration["Email:FrontendUrl"] ?? allowedOrigins.FirstOrDefault() ?? "http://localhost:3000";
        var defaultCallbackUrl = defaultFrontendUrl.TrimEnd('/') + "/callback";

        if (string.IsNullOrWhiteSpace(requestedRedirectUrl))
        {
            return defaultCallbackUrl;
        }

        if (!Uri.TryCreate(requestedRedirectUrl, UriKind.Absolute, out var redirectUri))
        {
            return defaultCallbackUrl;
        }

        var redirectOrigin = GetOrigin(redirectUri);
        var isAllowed = allowedOrigins.Any(origin => string.Equals(origin.TrimEnd('/'), redirectOrigin, StringComparison.OrdinalIgnoreCase));

        return isAllowed ? redirectUri.ToString() : defaultCallbackUrl;
    }

    private static string BuildCallbackRedirectUrl(string callbackUrl, string token)
    {
        if (!Uri.TryCreate(callbackUrl, UriKind.Absolute, out var callbackUri))
        {
            return callbackUrl;
        }

        var builder = new UriBuilder(callbackUri)
        {
            Fragment = "token=" + Uri.EscapeDataString(token)
        };

        return builder.Uri.ToString();
    }

    private static string GetOrigin(Uri uri)
    {
        var port = uri.IsDefaultPort ? string.Empty : ":" + uri.Port;
        return uri.Scheme + "://" + uri.Host + port;
    }

    private static async Task<IResult> Register(
        RegisterRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(
            Email: request.Email,
            Password: request.Password,
            DisplayName: request.DisplayName,
            Avatar: request.Avatar);

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
                Avatar: result.User.Avatar,
                Email: result.User.Email,
                Provider: result.User.Provider.ToString().ToLowerInvariant(),
                HighScore: result.User.HighScore,
                GamesPlayed: result.User.GamesPlayed,
                Role: result.User.Role.ToString()),
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
