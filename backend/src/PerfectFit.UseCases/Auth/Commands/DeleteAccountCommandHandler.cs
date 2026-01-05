using MediatR;
using PerfectFit.Core.Interfaces;

namespace PerfectFit.UseCases.Auth.Commands;

/// <summary>
/// Handler for the DeleteAccountCommand.
/// Permanently deletes a user account and all associated data.
/// </summary>
public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, DeleteAccountResult>
{
    private readonly IUserRepository _userRepository;

    public DeleteAccountCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<DeleteAccountResult> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return DeleteAccountResult.Failed("User not found.");
        }

        await _userRepository.DeleteAsync(user, cancellationToken);

        return DeleteAccountResult.Succeeded();
    }
}
