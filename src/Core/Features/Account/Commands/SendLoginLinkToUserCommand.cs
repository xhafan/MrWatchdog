using CoreBackend.Features.Account.Commands;
using System.Globalization;

namespace MrWatchdog.Core.Features.Account.Commands;

public record SendLoginLinkToUserCommand(
    string Email,
    CultureInfo Culture,
    string? ReturnUrl
) 
    : BaseSendLoginLinkToUserCommand(Email, ReturnUrl);