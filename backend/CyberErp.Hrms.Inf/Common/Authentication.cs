using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Features.Core.Users.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CyberErp.Hrms.Inf.Common
{
    public class Authentication(IConfiguration configuration) : IAuthentication
    {
        private readonly IConfiguration _configuration = configuration;

        public string EncryptPassword(string password)
        {
            var hashedPassword = Encryption.GenerateHash(password);
            return hashedPassword;
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            var isExist = Encryption.VerifyHash(password, hashedPassword);
            return isExist;
        }
        public string GenerateToken(UserResult user, Guid tokenId)
        {
            long iat = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            var subject = _configuration["Jwt:Subject"];
            var claims = new[] {
                    new Claim(JwtRegisteredClaimNames.Sub,subject!),
                    new Claim(JwtRegisteredClaimNames.Jti, tokenId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, iat.ToString()),
                    new Claim("UserId", user.Id.ToString()),
                    new Claim("FullName", user.FullName),
                    new Claim("Email", user.Email),
                    new Claim("PhoneNo", user.PhoneNumber),
                    new Claim("UserName", user.UserName),
                    new Claim("TenantId", user.TenantId.HasValue ? user.TenantId.Value.ToString() : Guid.Empty.ToString())
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

        public JwtSecurityToken ParseToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token cannot be null or empty.", nameof(token));

            var tokenHandler = new JwtSecurityTokenHandler();

            if (!tokenHandler.CanReadToken(token))
                throw new SecurityTokenException("Invalid JWT format.");

            var jwtToken = tokenHandler.ReadJwtToken(token);

            if (jwtToken == null)
                throw new SecurityTokenException("Failed to parse JWT.");

            return jwtToken;
        }
    }
}

