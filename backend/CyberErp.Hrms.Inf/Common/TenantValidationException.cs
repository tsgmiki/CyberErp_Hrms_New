namespace CyberErp.Hrms.Inf.Common
{
    /// <summary>
    /// Exception thrown when tenant validation fails (null tenantId or expired subscription).
    /// </summary>
    public class TenantValidationException : Exception
    {
        public TenantValidationException(string message) : base(message)
        {
        }

        public TenantValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
