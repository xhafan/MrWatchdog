using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Jobs.Domain;

namespace MrWatchdog.Core.Features.Jobs.Queries;

public class GetRelatedDomainEventJobQueryHandler(
    NhibernateUnitOfWork unitOfWork
) : BaseNhibernateQueryHandler<GetRelatedDomainEventJobQuery>(unitOfWork)
{
    public override async Task<IEnumerable<TResult>> ExecuteAsync<TResult>(GetRelatedDomainEventJobQuery query)
    {
        Job relatedCommandJob = null!;
        
        var domainEventJob = await Session.QueryOver<Job>()
            .JoinAlias(x => x.RelatedCommandJob, () => relatedCommandJob)
            .Where(x => relatedCommandJob.Guid == query.CommandJobGuid
                        && x.Type == query.Type)
            .SingleOrDefaultAsync();
        return domainEventJob == null
            ? []
            : [(TResult) (object) domainEventJob.GetDto()];
    }
}