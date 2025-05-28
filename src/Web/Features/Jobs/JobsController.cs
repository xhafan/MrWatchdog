using CoreDdd.Queries;
using CoreUtils.Extensions;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Jobs.Queries;

namespace MrWatchdog.Web.Features.Jobs;

[ApiController]
[Route("api/[controller]")]
public class JobsController(IQueryExecutor queryExecutor) : ControllerBase
{
    [HttpGet("{jobGuid}")]
    public async Task<IActionResult> GetJob(Guid jobGuid)
    {
        var jobDtos = (
            await queryExecutor.ExecuteAsync<GetJobQuery, JobDto>(new GetJobQuery(jobGuid))
        ).ToList();

        return jobDtos.IsEmpty()
            ? NotFound()
            : Ok(jobDtos.Single());
    }
}