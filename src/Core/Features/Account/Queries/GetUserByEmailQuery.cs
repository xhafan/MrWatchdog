using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Account.Queries;

public record GetUserByEmailQuery(string Email) : IQuery;