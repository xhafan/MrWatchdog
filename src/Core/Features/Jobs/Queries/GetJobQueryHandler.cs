﻿using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Infrastructure.Repositories;

namespace MrWatchdog.Core.Features.Jobs.Queries;

public class GetJobQueryHandler(
    NhibernateUnitOfWork unitOfWork, 
    IJobRepository jobRepository
) : BaseNhibernateQueryHandler<GetJobQuery>(unitOfWork)
{
    public override async Task<IEnumerable<TResult>> ExecuteAsync<TResult>(GetJobQuery query)
    {
        var job = await jobRepository.GetByGuidAsync(query.JobGuid);
        return job == null
            ? []
            : [(TResult) (object) job.GetDto()];
    }
}