using CoreBackend.Messages;

namespace CoreBackend.Features.Account.Commands;

public abstract record BaseSendLoginLinkToUserCommand(
    string Email,
    string? ReturnUrl
) : Command;