using AIZ_MVP_Bussiness.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AIZ_MVP_Presentation.Extensions
{
    public class JwtTokenProvider : ITokenProvider
    {
        private readonly JwtSettings _jwtSettings;
        public JwtTokenProvider(IOptions<JwtSettings> jwtOptions)
        {
            _jwtSettings = jwtOptions.Value;
        }
        public string GenerateAccessToken(string userId, string username, string role)
        {
            var jwtKey = _jwtSettings.Key
    ?? throw new InvalidOperationException("JWT Key is not configured.");

            // derive 32 bytes key for HS256
            var key = SHA256.HashData(Encoding.UTF8.GetBytes(jwtKey));

            var expireString = _jwtSettings.ExpireMinutes
                ?? throw new InvalidOperationException("JWT ExpireMinutes is not configured.");
            var expireMinutes = int.Parse(expireString);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expireMinutes),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }

}
