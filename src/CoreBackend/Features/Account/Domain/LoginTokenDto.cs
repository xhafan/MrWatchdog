using Reinforced.Typings.Attributes;

namespace CoreBackend.Features.Account.Domain;

[TsInterface]
public record LoginTokenDto(
    Guid Guid,
    string Token,
    bool Confirmed,
    bool Used
);