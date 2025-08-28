using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.TestsShared.Builders;

public class LoginTokenBuilder(NhibernateUnitOfWork? unitOfWork = null)
{
    public const string? TokenReturnUrl = "/Watchdogs/Alerts";
    
    private Guid _guid;
    private string _email = $"user+{Guid.NewGuid()}@email.com";
    private string? _token;
    private string? _tokenReturnUrl = TokenReturnUrl;

    public LoginTokenBuilder WithGuid(Guid guid)
    {
        _guid = guid;
        return this;
    }
    
    public LoginTokenBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public LoginTokenBuilder WithToken(string token)
    {
        _token = token;
        return this;
    }
    
    public LoginTokenBuilder WithTokenReturnUrl(string? tokenReturnUrl)
    {
        _tokenReturnUrl = tokenReturnUrl;
        return this;
    }    
    
    public LoginToken Build()
    {
        if (_guid == Guid.Empty)
        {
            _guid = Guid.NewGuid();
        }

        if (string.IsNullOrWhiteSpace(_token))
        {
            _token = TokenGenerator.GenerateToken(
                _guid, 
                _email, 
                _tokenReturnUrl, 
                OptionsRetriever.Retrieve<JwtOptions>().Value
            );
        }
        
        var loginToken = new LoginToken(
            _guid,
            _email,
            _token
        );

        unitOfWork?.Save(loginToken);
        
        return loginToken;
    }
}