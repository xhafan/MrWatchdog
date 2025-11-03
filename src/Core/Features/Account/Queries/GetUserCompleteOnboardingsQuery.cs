using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Account.Queries;

public record GetUserCompleteOnboardingsQuery(long UserId) : IQuery;