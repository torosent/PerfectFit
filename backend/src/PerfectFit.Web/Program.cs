using PerfectFit.Infrastructure;
using PerfectFit.UseCases.Games.Commands;
using PerfectFit.Web.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add infrastructure services (DbContext, Repositories)
builder.Services.AddInfrastructure(builder.Configuration);

// Add MediatR for CQRS
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(CreateGameCommand).Assembly));

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

app.Run();

// Make the implicit Program class public so test projects can access it
public partial class Program { }
