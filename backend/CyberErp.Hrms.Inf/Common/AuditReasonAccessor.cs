using CyberErp.Hrms.App.Common.Services;

namespace CyberErp.Hrms.Inf.Common
{
    /// <inheritdoc cref="IAuditReasonAccessor"/>
    public class AuditReasonAccessor : IAuditReasonAccessor
    {
        private string? _reason;

        public void Set(string? reason) =>
            _reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();

        public string? Consume()
        {
            var value = _reason;
            // Cleared on read, so a reason can never bleed onto the next save in the same request.
            _reason = null;
            return value;
        }
    }
}
