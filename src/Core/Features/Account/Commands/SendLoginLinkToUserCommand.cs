using System.Globalization;
using CoreBackend.Messages;

namespace MrWatchdog.Core.Features.Account.Commands;

public record SendLoginLinkToUserCommand(
    string Email,
    CultureInfo Culture,
    string? ReturnUrl
) : Command;