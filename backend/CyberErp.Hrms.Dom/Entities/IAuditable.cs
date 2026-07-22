namespace CyberErp.Hrms.Dom.Entities
{
    /// <summary>
    /// Marker for entities whose structural mutations (create / modify / reassign / delete)
    /// must be captured in the audit trail. The EF SaveChanges interceptor writes an
    /// AuditLog row for every tracked change to an <see cref="IAuditable"/> entity.
    /// </summary>
    public interface IAuditable
    {
    }
}
