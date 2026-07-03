using CyberErp.Hrms.App.Features.Core.Users.DTOs;
using System.IdentityModel.Tokens.Jwt;

namespace CyberErp.Hrms.App.Common.Services
{
    public interface IAuthentication
    {
        string EncryptPassword(string password);
        string GenerateToken(UserResult user, Guid tokenId);
        bool VerifyPassword(string password, string hashedPassword);
        JwtSecurityToken ParseToken(string token);
    }
}

