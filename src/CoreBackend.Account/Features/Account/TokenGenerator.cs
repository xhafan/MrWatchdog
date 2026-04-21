using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace CoreBackend.Account.Features.Account;

public static class TokenGenerator
{
    public static string GenerateLoginToken(
        Guid tokenGuid, 
        string email,
        IEnumerable<Claim> customClaims,
        string? returnUrl,
        JwtOptions jwtOptions,
        DateTime? validFrom = null
    )
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));
        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>(customClaims)
        {
            new(ClaimTypes.Email, email),
            new(CoreBackendClaimTypes.Guid, tokenGuid.ToString())
        };
        
        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            claims.Add(new Claim(CoreBackendClaimTypes.ReturnUrl, returnUrl));
        }        

        validFrom ??= DateTime.UtcNow;
        
        var token = new JwtSecurityToken(
            claims: claims,
            notBefore: validFrom,
            expires: validFrom.Value.AddMinutes(jwtOptions.ExpireMinutes),
            signingCredentials: signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string GenerateUnsubscribeToken(
        IEnumerable<Claim> claims,
        JwtOptions jwtOptions
    )
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));
        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            signingCredentials: signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}