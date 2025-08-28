using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Account.Queries;

public record GetLoginTokenConfirmationQuery(Guid LoginTokenGuid) : IQuery;