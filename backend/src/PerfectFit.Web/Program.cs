using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using PerfectFit.Infrastructure;
using PerfectFit.Infrastructure.Identity;
using PerfectFit.UseCases.Games.Commands;
using PerfectFit.Web.Endpoints;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configure JSON serialization for camelCase properties and string enums
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Add infrastructure services (DbContext, Repositories, JWT)
builder.Services.AddInfrastructure(builder.Configuration);

// Add database migration hosted service for automatic migrations with retry logic
builder.Services.AddDatabaseMigration();

// Add MediatR for CQRS
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateGameCommand).Assembly));

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()!;
var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);

var authBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

// Configure OAuth providers only if credentials are provided
var googleSettings = builder.Configuration.GetSection("OAuth:Google").Get<GoogleSettings>();
if (!string.IsNullOrEmpty(googleSettings?.ClientId) && !string.IsNullOrEmpty(googleSettings?.ClientSecret))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleSettings.ClientId;
        options.ClientSecret = googleSettings.ClientSecret;
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    });
}

var msSettings = builder.Configuration.GetSection("OAuth:Microsoft").Get<MicrosoftSettings>();
if (!string.IsNullOrEmpty(msSettings?.ClientId) && !string.IsNullOrEmpty(msSettings?.ClientSecret))
{
    authBuilder.AddMicrosoftAccount(options =>
    {
        options.ClientId = msSettings.ClientId;
        options.ClientSecret = msSettings.ClientSecret;
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    });
}

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => 
        policy.RequireClaim(System.Security.Claims.ClaimTypes.Role, "Admin"));
});

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "PerfectFit API",
        Version = "v1",
        Description = "Block puzzle game API"
    });

    // Add JWT bearer authentication to Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure CORS
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:3000"];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add health checks
builder.Services.AddHealthChecks();

// Add rate limiting for profile update endpoint
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("ProfileUpdateLimit", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "PerfectFit API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors();

// Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Rate limiting middleware (after auth so user claims are available)
app.UseRateLimiter();

// Health check endpoint
app.MapHealthChecks("/health");

// API status endpoint
app.MapGet("/api/status", () => Results.Ok(new
{
    Status = "Healthy",
    Service = "PerfectFit API",
    Version = "1.0.0",
    Timestamp = DateTime.UtcNow
}))
.WithName("GetStatus");

// Map game endpoints
app.MapGameEndpoints();

// Map auth endpoints
app.MapAuthEndpoints();

// Map leaderboard endpoints
app.MapLeaderboardEndpoints();

// Map admin endpoints
app.MapAdminEndpoints();

app.Run();

// Make the implicit Program class public so test projects can access it
public partial class Program { }
