using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace CoreBackend.Features.Account;

public static class TokenValidator
{
    public static ClaimsPrincipal ValidateToken(
        string tokenString, 
        JwtOptions jwtOptions,
        bool validateLifetime = true
    )
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(jwtOptions.Key);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = validateLifetime,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        return tokenHandler.ValidateToken(tokenString, validationParameters, out _);
    }
}