using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MrWatchdog.Core.Features.Account;

public static class TokenGenerator
{
    public static string GenerateLoginToken(
        Guid tokenGuid, 
        string email,
        string cultureName,
        string? returnUrl,
        JwtOptions jwtOptions,
        DateTime? validFrom = null
    )
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));
        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, email),
            new(CustomClaimTypes.Guid, tokenGuid.ToString()),
            new(CustomClaimTypes.CultureName, cultureName)
        };
        
        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            claims.Add(new Claim(CustomClaimTypes.ReturnUrl, returnUrl));
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
        long watchdogId,
        JwtOptions jwtOptions
    )
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));
        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(CustomClaimTypes.WatchdogId, watchdogId.ToString())
        };
        
        var token = new JwtSecurityToken(
            claims: claims,
            signingCredentials: signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}