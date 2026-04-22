using Reinforced.Typings.Attributes;

namespace CoreBackend.Account.Features.LoginLink.Domain;

[TsInterface]
public record LoginTokenDto(
    Guid Guid,
    string Token,
    bool Confirmed,
    bool Used
);