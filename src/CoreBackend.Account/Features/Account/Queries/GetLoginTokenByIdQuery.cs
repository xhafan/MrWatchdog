using CoreBackend.Account.Features.Account.Domain;
using CoreDdd.Queries;

namespace CoreBackend.Account.Features.Account.Queries;

public record GetLoginTokenByIdQuery(long LoginTokenId) : IQuery<LoginTokenDto>;