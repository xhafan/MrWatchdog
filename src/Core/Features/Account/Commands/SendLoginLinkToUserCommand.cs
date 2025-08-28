using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Account.Commands;

public record SendLoginLinkToUserCommand(
    string Email,
    string? ReturnUrl
) : Command;