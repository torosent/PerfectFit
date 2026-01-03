using MediatR;

namespace PerfectFit.UseCases.Auth.Commands;

/// <summary>
/// Command to update user profile (username and/or avatar).
/// </summary>
public record UpdateProfileCommand(int UserId, string? Username, string? Avatar) : IRequest<UpdateProfileResult>;
