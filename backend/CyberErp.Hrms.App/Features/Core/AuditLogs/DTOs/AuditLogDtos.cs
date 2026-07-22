namespace CyberErp.Hrms.App.Features.Core.AuditLogs.DTOs
{
    public class AuditLogDto
    {
        public Guid Id { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string? EntityName { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Changes { get; set; }
        public Guid? PerformedByUserId { get; set; }
        public string? PerformedBy { get; set; }
        public Guid? BranchId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
