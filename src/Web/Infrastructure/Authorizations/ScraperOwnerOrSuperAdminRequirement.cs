using Microsoft.AspNetCore.Authorization;

namespace MrWatchdog.Web.Infrastructure.Authorizations;

public class ScraperOwnerOrSuperAdminRequirement : IAuthorizationRequirement;