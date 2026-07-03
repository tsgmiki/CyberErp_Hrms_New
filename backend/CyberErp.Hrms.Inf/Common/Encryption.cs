using System.Security.Cryptography;

namespace CyberErp.Hrms.Inf.Common
{
    public static class Encryption
    {
        public static string GenerateHash(string password)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, new byte[0], 10000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32); // 256 bits
            return Convert.ToBase64String(hash);
        }

        public static bool VerifyHash(string password, string storedHash)
        {
            byte[] storedHashBytes = Convert.FromBase64String(storedHash);
            if (storedHashBytes.Length != 32)
            {
                return false;
            }
            using var pbkdf2 = new Rfc2898DeriveBytes(password, new byte[0], 10000, HashAlgorithmName.SHA256);
            byte[] computedHash = pbkdf2.GetBytes(32);
            return computedHash.SequenceEqual(storedHashBytes);
        }
    }
}
