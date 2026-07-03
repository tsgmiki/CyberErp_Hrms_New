using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Features.Core.Users.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CyberErp.Hrms.Inf.Common
{
    public class TokenStore : ITokenStore
    {
        private readonly ConcurrentDictionary<string, (bool Revoked, DateTime ExpiresAt)> _store = new();
        private readonly IConfiguration _configuration;

        public TokenStore(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(UserResult user)
        {
            long iat = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            var tokenId = Guid.NewGuid();

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]!),
                new Claim(JwtRegisteredClaimNames.Jti, tokenId.ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, iat.ToString()),
                new Claim("UserId", user.Id.ToString()),
                new Claim("FullName", user.FullName),
                new Claim("Email", user.Email),
                new Claim("PhoneNo", user.PhoneNumber),
                new Claim("UserName", user.UserName),
                new Claim("CompanyId", user.CompanyId != null ? user.CompanyId.ToString() : Guid.Empty.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: signIn);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();

            if (!tokenHandler.CanReadToken(token))
                return null;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out _);

                return principal;
            }
            catch
            {
                return null;
            }
        }

        public Task StoreAsync(string tokenId, DateTime expiresAt)
        {
            _store[tokenId] = (false, expiresAt);
            return Task.CompletedTask;
        }

        public Task RevokeAsync(string tokenId)
        {
            if (_store.ContainsKey(tokenId))
            {
                var current = _store[tokenId];
                _store[tokenId] = (true, current.ExpiresAt);
            }
            return Task.CompletedTask;
        }

        public Task<bool> IsRevokedAsync(string tokenId)
        {
            if (_store.TryGetValue(tokenId, out var entry))
            {
                var isExpired = entry.ExpiresAt < DateTime.UtcNow;
                return Task.FromResult(entry.Revoked || isExpired);
            }
            return Task.FromResult(true);
        }
    }
}

