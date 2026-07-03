
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace CyberErp.Hrms.Inf.Common
{
    public interface ITokenParser
    {
        public JwtSecurityToken ParseToken(string token);
    }
    public class TokenParser : ITokenParser
    {
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

