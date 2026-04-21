using CoreDdd.Queries;

namespace CoreBackend.Account.Features.Account.Queries;

public record GetLoginTokenConfirmationQuery(Guid LoginTokenGuid) : IQuery<bool>;