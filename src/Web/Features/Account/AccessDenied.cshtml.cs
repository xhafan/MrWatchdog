using CoreWeb.Features.Shared;
using Microsoft.AspNetCore.Authorization;

namespace MrWatchdog.Web.Features.Account;

[AllowAnonymous]
public class AccessDeniedModel : BasePageModel;
