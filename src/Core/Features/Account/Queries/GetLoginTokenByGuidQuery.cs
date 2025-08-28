using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Account.Queries;

public record GetLoginTokenByGuidQuery(Guid LoginTokenGuid) : IQuery;