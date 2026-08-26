namespace CyberErp.Hrms.App.Common.Services
{
    /// <summary>
    /// Hashes a password for storage. The implementation lives in Inf (it owns the crypto); this
    /// interface exists so App code can create accounts without referencing it.
    ///
    /// <para>⚠️ The current implementation derives the hash with an EMPTY salt, so identical
    /// passwords produce identical hashes. That is why generated passwords must be random per
    /// account — two employees given the same password would be visibly the same row.</para>
    /// </summary>
    public interface IPasswordHasher
    {
        string Hash(string password);
    }
}
