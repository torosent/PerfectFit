using MediatR;

namespace PerfectFit.UseCases.Auth.Commands;

/// <summary>
/// Command to delete a user account and all associated data.
/// </summary>
public record DeleteAccountCommand(int UserId) : IRequest<DeleteAccountResult>;

/// <summary>
/// Result of the delete account operation.
/// </summary>
public record DeleteAccountResult(bool Success, string? ErrorMessage = null)
{
    public static DeleteAccountResult Succeeded() => new(true);
    public static DeleteAccountResult Failed(string errorMessage) => new(false, errorMessage);
}
