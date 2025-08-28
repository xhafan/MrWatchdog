using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MrWatchdog.Core.Features.Account;

public static class TokenValidator
{
    public static ClaimsPrincipal ValidateToken(
        string tokenString, 
        JwtOptions jwtOptions
    )
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(jwtOptions.Key);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        return tokenHandler.ValidateToken(tokenString, validationParameters, out _);
    }
}