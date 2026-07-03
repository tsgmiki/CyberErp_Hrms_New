namespace CyberErp.Hrms.Api.Configuration
{
    public class JwtConfiguration
    {
        public string Subject { get; set; } = "CyberErpHrmsApi";
        public string Key { get; set; } = "YourSuperSecretKeyForJwtTokenGeneration12345";
        public string Issuer { get; set; } = "CyberErpHrmsApi";
        public string Audience { get; set; } = "CyberErpHrmsApiUsers";
        public int ExpirationInHours { get; set; } = 1;
    }
}
