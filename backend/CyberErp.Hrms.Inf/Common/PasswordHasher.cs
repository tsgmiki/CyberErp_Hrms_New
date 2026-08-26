using CyberErp.Hrms.App.Common.Services;

namespace CyberErp.Hrms.Inf.Common
{
    /// <summary>Wraps <see cref="Encryption"/> so App-layer code can hash without referencing Inf.</summary>
    public class PasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => Encryption.GenerateHash(password);
    }
}
