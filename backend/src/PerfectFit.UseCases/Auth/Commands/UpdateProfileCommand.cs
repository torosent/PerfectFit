using MediatR;

namespace PerfectFit.UseCases.Auth.Commands;

/// <summary>
/// Command to update user profile (display name and/or avatar).
/// </summary>
public record UpdateProfileCommand(int UserId, string? DisplayName, string? Avatar) : IRequest<UpdateProfileResult>;
