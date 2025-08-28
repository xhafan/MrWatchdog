using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MrWatchdog.Core.Features.Account;

public static class TokenGenerator
{
    public static string GenerateToken(
        Guid tokenGuid, 
        string email,
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
            new(CustomClaimTypes.Guid, tokenGuid.ToString())
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
}