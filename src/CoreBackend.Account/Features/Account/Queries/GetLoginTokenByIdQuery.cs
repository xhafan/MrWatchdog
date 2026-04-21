using CoreBackend.Features.Account.Domain;
using CoreDdd.Queries;

namespace CoreBackend.Features.Account.Queries;

public record GetLoginTokenByIdQuery(long LoginTokenId) : IQuery<LoginTokenDto>;