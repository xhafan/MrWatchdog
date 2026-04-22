using CoreBackend.Account.Features.LoginLink.Domain;
using CoreDdd.Queries;

namespace CoreBackend.Account.Features.LoginLink.Queries;

public record GetLoginTokenByIdQuery(long LoginTokenId) : IQuery<LoginTokenDto>;