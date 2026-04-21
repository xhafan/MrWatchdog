using CoreBackend.Features.Shared.Domain;
using CoreDdd.Domain;
using CoreUtils;

namespace CoreBackend.Features.Account.Domain;

public class LoginToken : VersionedEntity, IAggregateRoot
{
    protected LoginToken() {}

    public LoginToken(
        Guid guid,
        string email,
        string token
    )
    {
        Guid = guid;
        Email = email;
        Token = token;
    }

    public virtual Guid Guid { get; }
    public virtual string Email { get; } = null!;
    public virtual string Token { get; } = null!;
    public virtual bool Confirmed { get; protected set; }
    public virtual bool Used { get; protected set; }
    
    public virtual LoginTokenDto GetDto()
    {
        return new LoginTokenDto(
            Guid,
            Token,
            Confirmed,
            Used
        );
    }

    public virtual void Confirm()
    {
        Guard.Hope(!Confirmed, "Login token has already been confirmed.");
        
        Confirmed = true;
    }

    public virtual void MarkAsUsed()
    {
        Guard.Hope(Confirmed, "Login token has not been confirmed.");
        Guard.Hope(!Used, "Login token has been already used.");
        
        Used = true;
    }    
}