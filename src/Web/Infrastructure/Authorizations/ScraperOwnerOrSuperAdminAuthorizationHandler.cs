using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;

namespace MrWatchdog.Web.Infrastructure.Authorizations;

public class ScraperOwnerOrSuperAdminAuthorizationHandler(
    IUserRepository userRepository,
    IRepository<Scraper> scraperRepository
) 
    : AuthorizationHandler<ScraperOwnerOrSuperAdminRequirement, long>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ScraperOwnerOrSuperAdminRequirement requirement,
        long scraperId
    )
    {
        var scraper = await scraperRepository.LoadByIdAsync(scraperId);
        if (context.User.GetUserId() == scraper.User.Id
            || await context.User.IsSuperAdmin(userRepository))
        {
            context.Succeed(requirement);
        }
    }
}