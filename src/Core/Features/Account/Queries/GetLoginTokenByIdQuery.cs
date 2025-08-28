using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Account.Queries;

public record GetLoginTokenByIdQuery(long LoginTokenId) : IQuery;