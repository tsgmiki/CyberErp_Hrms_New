using CyberErp.Hrms.App.Common.Services;
using Microsoft.Extensions.Configuration;

namespace CyberErp.Hrms.Inf.Common
{
    /// <summary>
    /// Reads the two facts about the <c>Email</c> section the App layer is allowed to know.
    /// See <see cref="IEmailConfiguration"/> for why the password itself is not exposed.
    /// </summary>
    public class EmailConfiguration(IConfiguration configuration) : IEmailConfiguration
    {
        public bool Enabled => configuration.GetSection("Email").GetValue("Enabled", false);

        public bool HasPassword => !string.IsNullOrEmpty(configuration.GetSection("Email")["Password"]);
    }
}
