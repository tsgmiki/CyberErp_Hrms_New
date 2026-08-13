using CyberErp.Hrms.App.Common.Services;

namespace CyberErp.Hrms.Inf.Common
{
    /// <summary>
    /// Exposes the current tenant's key to the App layer, which cannot reference Finbuckle.
    ///
    /// <para>The Finbuckle discriminator is a STRING copy of <c>Core.Tenant.Id</c>, so parsing it back
    /// to a <see cref="Guid"/> is the whole job. It fails only when the tenant is unresolved (before
    /// sign-in, or in a background job) — callers treat null as "do nothing" rather than guessing.</para>
    /// </summary>
    public class CurrentTenantService(ITenantService tenantService) : ICurrentTenantService
    {
        public Guid? GetCurrentTenantId() =>
            Guid.TryParse(tenantService.GetCurrentTenantId(), out var id) && id != Guid.Empty
                ? id
                : null;
    }
}
