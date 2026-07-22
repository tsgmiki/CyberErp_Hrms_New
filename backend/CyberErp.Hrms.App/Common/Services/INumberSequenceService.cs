namespace CyberErp.Hrms.App.Common.Services
{
    /// <summary>
    /// Race-safe, per-tenant business-number counter (logic.md §7.1 adoption #5) — replaces
    /// count+1 numbering, which double-allocates under concurrent creates. Backed by an atomic
    /// UPDATE … OUTPUT on Core.hrms_NumberSequence.
    /// </summary>
    public interface INumberSequenceService
    {
        /// <summary>Atomically reserves and returns the next number for the tenant-scoped key.</summary>
        Task<long> NextAsync(string key);
    }
}
