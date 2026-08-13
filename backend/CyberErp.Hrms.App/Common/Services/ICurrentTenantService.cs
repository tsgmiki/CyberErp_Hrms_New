namespace CyberErp.Hrms.App.Common.Services
{
    /// <summary>
    /// The current tenant's identity as a <see cref="Guid"/>.
    ///
    /// <para>Most code never needs this: <c>IRepository&lt;T&gt;.GetAll()</c> already filters by the
    /// Finbuckle discriminator, so reads are tenant-scoped for free. It is needed only when a row
    /// carries a real FOREIGN KEY to <c>Core.Tenant</c> — the tenant-scoped authorization tables —
    /// because the discriminator is a string copy of that key, not the key itself.</para>
    /// </summary>
    public interface ICurrentTenantService
    {
        /// <summary>The current tenant's <c>Core.Tenant.Id</c>, or null outside a tenant context.</summary>
        Guid? GetCurrentTenantId();
    }
}
