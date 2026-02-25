using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Localization;
using NHibernate.Transform;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public class GetOtherUsersScrapersQueryHandler(
    NhibernateUnitOfWork unitOfWork
) : BaseNhibernateQueryHandler<GetOtherUsersScrapersQuery, GetOtherUsersScrapersQueryResult>(unitOfWork)
{
    private readonly NhibernateUnitOfWork _unitOfWork = unitOfWork;

    public override async Task<IEnumerable<GetOtherUsersScrapersQueryResult>> ExecuteAsync(GetOtherUsersScrapersQuery query)
    {
        User userAlias = null!;
        GetOtherUsersScrapersQueryResult result = null!;

        var results = await _unitOfWork.Session!.QueryOver<Scraper>()
            .JoinAlias(x => x.User, () => userAlias)
            .Where(x => userAlias.Id != query.UserId
                        && !x.IsArchived)
            .SelectList(list => list
                .Select(x => x.Id).WithAlias(() => result.ScraperId)
                .Select(x => x.Name).WithAlias(() => result.ScraperName)
                .Select(x => x.PublicStatus).WithAlias(() => result.PublicStatus)
                .Select(() => userAlias.Id).WithAlias(() => result.UserId)
                .Select(() => userAlias.Email).WithAlias(() => result.UserEmail)
            )
            .TransformUsing(Transformers.AliasToBean<GetOtherUsersScrapersQueryResult>())
            .ListAsync<GetOtherUsersScrapersQueryResult>();

        var localizedResults = results.Select(x => x with
            {
                ScraperName = LocalizedTextResolver.ResolveLocalizedText(x.ScraperName, query.Culture)
            })
            .OrderBy(x => x.ScraperName)
            .ToList();
        
        return localizedResults;
    }
}