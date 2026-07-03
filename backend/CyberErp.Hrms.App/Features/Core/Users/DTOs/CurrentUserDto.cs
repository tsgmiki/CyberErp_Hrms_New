namespace CyberErp.Hrms.App.Features.Core.Users.DTOs
{
    public class CurrentUserResult
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? TenantId { get; set; }
        public bool IsAuthenticated { get; set; }
    }
}
