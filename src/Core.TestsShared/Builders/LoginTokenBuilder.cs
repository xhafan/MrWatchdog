using CoreBackend.TestsShared;
using CoreDdd.Nhibernate.TestHelpers;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account;
using MrWatchdog.Core.Infrastructure.Localization;
using System.Globalization;
using System.Security.Claims;
using CoreBackend.Account.Features.Account;
using CoreBackend.Account.Features.Account.Domain;

namespace MrWatchdog.Core.TestsShared.Builders;

public class LoginTokenBuilder(NhibernateUnitOfWork? unitOfWork = null)
{
    public const string? TokenReturnUrl = "/Watchdogs";
    public static readonly CultureInfo Culture = CultureConstants.En;
    
    private Guid _guid;
    private string _email = $"user+{Guid.NewGuid()}@email.com";
    private string? _token;
    private string? _tokenReturnUrl = TokenReturnUrl;
    private CultureInfo _culture = Culture;

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
    
    public LoginTokenBuilder WithCulture(CultureInfo culture)
    {
        _culture = culture;
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
            _token = TokenGenerator.GenerateLoginToken(
                _guid, 
                _email, 
                [new Claim(CustomClaimTypes.CultureName, _culture.Name)],
                _tokenReturnUrl, 
                OptionsTestRetriever.Retrieve<JwtOptions>().Value
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