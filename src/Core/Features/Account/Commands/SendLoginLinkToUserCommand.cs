using System.Globalization;
using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Account.Commands;

public record SendLoginLinkToUserCommand(
    string Email,
    CultureInfo Culture,
    string? ReturnUrl
) : Command;