namespace CyberErp.Hrms.App.Common.Services
{
    /// <summary>
    /// The parts of the <c>Email</c> configuration section the application needs to reason about but
    /// must never hold: whether mail is switched on for the deployment, and whether a password exists.
    ///
    /// <para>⚠️ There is no accessor for the password value itself, on purpose. It is read once, at
    /// the point of the send, inside <c>SmtpEmailService</c> — nothing else should be able to reach
    /// it, and nothing should be able to return it to a client by accident.</para>
    /// </summary>
    public interface IEmailConfiguration
    {
        /// <summary>The <c>Email:Enabled</c> master switch.</summary>
        bool Enabled { get; }

        /// <summary>True when <c>Email:Password</c> is set. Never the value.</summary>
        bool HasPassword { get; }
    }
}
