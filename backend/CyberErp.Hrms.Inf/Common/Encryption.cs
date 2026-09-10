using System.Security.Cryptography;

namespace CyberErp.Hrms.Inf.Common
{
    /// <summary>
    /// Password hashing.
    ///
    /// <para>⚠️ TWO FORMATS LIVE HERE AT ONCE, and that is deliberate. The original scheme was
    /// PBKDF2-SHA256 with an EMPTY salt, which means two people who chose the same password have the
    /// same stored hash — anyone who can read the table can see who shares a password, and one
    /// cracked hash opens every account that shares it. New hashes carry a per-user random salt;
    /// the old ones are still verifiable so nobody is locked out, and each is rewritten in the new
    /// format the next time its owner signs in successfully (logic §12.90).</para>
    ///
    /// <para>The legacy path exists only to let a correct password through once. It cannot be used to
    /// create a new hash: <see cref="GenerateHash"/> always writes the salted format.</para>
    /// </summary>
    public static class Encryption
    {
        private const int Iterations = 10000;
        private const int KeyBytes = 32;
        private const int SaltBytes = 16;
        /// <summary>Marks the salted format. A legacy hash is a bare 32-byte base64 string.</summary>
        private const string SaltedPrefix = "v2:";

        /// <summary>Always produces the salted format.</summary>
        public static string GenerateHash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltBytes);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(KeyBytes);
            return $"{SaltedPrefix}{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        /// <summary>Verifies against either format.</summary>
        public static bool VerifyHash(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash)) return false;

            return storedHash.StartsWith(SaltedPrefix, StringComparison.Ordinal)
                ? VerifySalted(password, storedHash)
                : VerifyLegacy(password, storedHash);
        }

        /// <summary>
        /// True when the stored hash is still in the old unsalted format, so the caller can rewrite it
        /// after a successful sign-in. Checked rather than assumed, so a rehash happens exactly once
        /// per account.
        /// </summary>
        public static bool NeedsRehash(string storedHash) =>
            !string.IsNullOrEmpty(storedHash) && !storedHash.StartsWith(SaltedPrefix, StringComparison.Ordinal);

        private static bool VerifySalted(string password, string storedHash)
        {
            var body = storedHash[SaltedPrefix.Length..];
            var separator = body.IndexOf('.');
            if (separator <= 0) return false;

            byte[] salt, expected;
            try
            {
                salt = Convert.FromBase64String(body[..separator]);
                expected = Convert.FromBase64String(body[(separator + 1)..]);
            }
            catch (FormatException)
            {
                return false;
            }
            if (expected.Length != KeyBytes) return false;

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            // Fixed-time comparison: a length-dependent early exit on a password check is a timing
            // oracle, and this is the one comparison in the product that guards every account.
            return CryptographicOperations.FixedTimeEquals(pbkdf2.GetBytes(KeyBytes), expected);
        }

        private static bool VerifyLegacy(string password, string storedHash)
        {
            byte[] expected;
            try { expected = Convert.FromBase64String(storedHash); }
            catch (FormatException) { return false; }
            if (expected.Length != KeyBytes) return false;

            using var pbkdf2 = new Rfc2898DeriveBytes(password, Array.Empty<byte>(), Iterations, HashAlgorithmName.SHA256);
            return CryptographicOperations.FixedTimeEquals(pbkdf2.GetBytes(KeyBytes), expected);
        }
    }
}
