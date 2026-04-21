using CoreBackend.Messages;

namespace CoreBackend.Account.Features.Account.Commands;

public abstract record BaseSendLoginLinkToUserCommand(
    string Email,
    string? ReturnUrl
) : Command;