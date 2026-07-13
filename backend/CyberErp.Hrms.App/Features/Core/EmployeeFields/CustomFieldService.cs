using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.EmployeeFields
{
    /// <summary>
    /// Shared custom-field engine (HC021) used by the Employee form and every employee child form.
    /// Validates submitted values against the active definitions for an owner type (unknown names
    /// rejected, required fields enforced) and upserts one value row per field. All writes are
    /// <b>staged only</b> (no SaveChanges) so the caller commits the owner record and its custom-field
    /// values in a single transaction (repositories share the scoped DbContext).
    /// </summary>
    public interface ICustomFieldService
    {
        /// <summary>Upserts the submitted values for one owner record. A null dictionary is a no-op
        /// (partial API clients leave stored values untouched).</summary>
        Task ApplyAsync(EmployeeFieldOwnerType ownerType, Guid ownerId, Dictionary<string, string?>? submitted);

        /// <summary>Reads the stored values for one owner record as a {fieldName → value} map.</summary>
        Task<Dictionary<string, string?>> GetValuesAsync(EmployeeFieldOwnerType ownerType, Guid ownerId);

        /// <summary>Bulk-reads values for many owner records at once (avoids N+1 on child-list reads):
        /// {ownerId → {fieldName → value}}.</summary>
        Task<Dictionary<Guid, Dictionary<string, string?>>> GetValuesForOwnersAsync(
            EmployeeFieldOwnerType ownerType, IReadOnlyCollection<Guid> ownerIds);

        /// <summary>Stages deletion of every value row for one owner record (call from the owner's
        /// delete handler — the value table is polymorphic with no cascade FK).</summary>
        Task DeleteForOwnerAsync(EmployeeFieldOwnerType ownerType, Guid ownerId);
    }

    public class CustomFieldService(
        IRepository<EmployeeFieldDefinition> definitions,
        IRepository<EmployeeFieldValue> values) : ICustomFieldService
    {
        public async Task ApplyAsync(EmployeeFieldOwnerType ownerType, Guid ownerId, Dictionary<string, string?>? submitted)
        {
            if (submitted is null) return;

            var activeDefs = await definitions.GetAll()
                .Where(d => d.IsActive && d.OwnerType == ownerType)
                .ToListAsync();
            var defsByName = activeDefs.ToDictionary(d => d.Name, StringComparer.OrdinalIgnoreCase);

            var unknown = submitted.Keys.Where(k => !defsByName.ContainsKey(k)).ToList();
            if (unknown.Count > 0)
                throw new ValidationException("customFields", $"Unknown custom field(s): {string.Join(", ", unknown)}");

            foreach (var def in activeDefs.Where(d => d.IsRequired))
            {
                submitted.TryGetValue(def.Name, out var v);
                if (string.IsNullOrWhiteSpace(v))
                    throw new ValidationException(def.Name, $"'{def.Label}' is required.");
            }

            var existing = await values.GetAll()
                .Where(v => v.OwnerType == ownerType && v.OwnerId == ownerId)
                .ToListAsync();

            foreach (var (name, value) in submitted)
            {
                var def = defsByName[name];
                var row = existing.FirstOrDefault(v => v.FieldDefinitionId == def.Id);
                if (row is null)
                {
                    await values.AddAsync(EmployeeFieldValue.Create(ownerType, ownerId, def.Id, value));
                }
                else if (row.Value != value)
                {
                    row.SetValue(value);
                    values.UpdateAsync(row);
                }
            }
        }

        public async Task<Dictionary<string, string?>> GetValuesAsync(EmployeeFieldOwnerType ownerType, Guid ownerId)
        {
            return await values.GetAll()
                .Where(v => v.OwnerType == ownerType && v.OwnerId == ownerId)
                .Join(definitions.GetAll(),
                    v => v.FieldDefinitionId, d => d.Id,
                    (v, d) => new { d.Name, v.Value })
                .ToDictionaryAsync(x => x.Name, x => x.Value);
        }

        public async Task<Dictionary<Guid, Dictionary<string, string?>>> GetValuesForOwnersAsync(
            EmployeeFieldOwnerType ownerType, IReadOnlyCollection<Guid> ownerIds)
        {
            if (ownerIds.Count == 0) return new Dictionary<Guid, Dictionary<string, string?>>();

            var rows = await values.GetAll()
                .Where(v => v.OwnerType == ownerType && ownerIds.Contains(v.OwnerId))
                .Join(definitions.GetAll(),
                    v => v.FieldDefinitionId, d => d.Id,
                    (v, d) => new { v.OwnerId, d.Name, v.Value })
                .ToListAsync();

            return rows
                .GroupBy(r => r.OwnerId)
                .ToDictionary(g => g.Key, g => g.ToDictionary(x => x.Name, x => x.Value));
        }

        public async Task DeleteForOwnerAsync(EmployeeFieldOwnerType ownerType, Guid ownerId)
        {
            var rows = await values.GetAll()
                .Where(v => v.OwnerType == ownerType && v.OwnerId == ownerId)
                .ToListAsync();
            foreach (var row in rows) values.Delete(row);
        }
    }
}
