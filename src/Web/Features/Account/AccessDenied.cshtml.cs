using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Account;

[AllowAnonymous]
public class AccessDeniedModel : BasePageModel;
