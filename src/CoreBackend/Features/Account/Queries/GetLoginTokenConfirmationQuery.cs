using CoreDdd.Queries;

namespace CoreBackend.Features.Account.Queries;

public record GetLoginTokenConfirmationQuery(Guid LoginTokenGuid) : IQuery<bool>;