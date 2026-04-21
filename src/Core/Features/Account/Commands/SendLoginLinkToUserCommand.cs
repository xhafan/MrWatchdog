using System.Globalization;
using CoreBackend.Account.Features.Account.Commands;

namespace MrWatchdog.Core.Features.Account.Commands;

public record SendLoginLinkToUserCommand(
    string Email,
    CultureInfo Culture,
    string? ReturnUrl
) 
    : BaseSendLoginLinkToUserCommand(Email, ReturnUrl);