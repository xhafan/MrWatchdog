using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features;

[AllowAnonymous]
public class SupportModel : BasePageModel;