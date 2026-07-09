using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BookTracker.Api.Application.Auth.Login;
using BookTracker.Api.Domain.Members;
using Microsoft.IdentityModel.Tokens;

namespace BookTracker.Api.Security;

public class JwtTokenGenerator(JwtSettings settings)
{
    public LoginResponse Generate(Member member)
    {
        var expiresAt =
            DateTime.UtcNow.AddMinutes(settings.ExpirationMinutes); // 9:00 >>> + 60 >>> 10:00

        var claims =
            new List<Claim> // Info in JWT (token access)
            {
                new(ClaimTypes.NameIdentifier, member.Id.ToString()),
                new(ClaimTypes.Name, member.Name.Value),
                new(ClaimTypes.Email, member.Email.Value)
            };

        var signingKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(settings.SigningKey));

        var credentials =
            new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token =
            new JwtSecurityToken(
                issuer: settings.Issuer,
                audience: settings.Audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

        var value = new JwtSecurityTokenHandler().WriteToken(token); // To String 

        return new LoginResponse
        {
            AccessToken = value,
            ExpiresAt = expiresAt
        };
    }
}