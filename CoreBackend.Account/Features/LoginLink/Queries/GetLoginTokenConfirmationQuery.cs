using CoreDdd.Queries;

namespace CoreBackend.Account.Features.LoginLink.Queries;

public record GetLoginTokenConfirmationQuery(Guid LoginTokenGuid) : IQuery<bool>;