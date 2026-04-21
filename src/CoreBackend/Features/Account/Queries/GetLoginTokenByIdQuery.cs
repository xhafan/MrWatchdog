using CoreDdd.Queries;
using MrWatchdog.Core.Features.Account.Domain;

namespace MrWatchdog.Core.Features.Account.Queries;

public record GetLoginTokenByIdQuery(long LoginTokenId) : IQuery<LoginTokenDto>;