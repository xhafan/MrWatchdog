using CoreBackend.Messages;

namespace CoreBackend.Account.Features.LoginLink.Commands;

public abstract record BaseSendLoginLinkToUserCommand(
    string Email,
    string? ReturnUrl
) : Command;