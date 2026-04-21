using System.Globalization;
using CoreBackend.Account.Features.LoginLink.Commands;

namespace MrWatchdog.Core.Features.Account.Commands;

public record SendLoginLinkToUserCommand(
    string Email,
    CultureInfo Culture,
    string? ReturnUrl
) 
    : BaseSendLoginLinkToUserCommand(Email, ReturnUrl);