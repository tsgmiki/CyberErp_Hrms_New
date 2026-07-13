using System.Text.Json;
using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Features.Core.Employees;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.DynamicForms
{
    // ---- DTOs ---------------------------------------------------------------
    public class DynamicFormFieldDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string DataType { get; set; } = nameof(EmployeeFieldDataType.Text);
        public string? Options { get; set; }
        public bool IsRequired { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
        public bool ShowInList { get; set; } = true;
    }

    public class DynamicFormDto
    {
        public Guid Id { get; set; }
        public string Module { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
        public List<DynamicFormFieldDto> Fields { get; set; } = [];
    }

    public class SaveDynamicFormFieldDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string DataType { get; set; } = nameof(EmployeeFieldDataType.Text);
        public string? Options { get; set; }
        public bool IsRequired { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
        public bool ShowInList { get; set; } = true;
    }

    public class SaveDynamicFormDto
    {
        public Guid? Id { get; set; }
        public string Module { get; set; } = "Employee";
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
        public List<SaveDynamicFormFieldDto> Fields { get; set; } = [];
    }

    public class DynamicFormRecordDto
    {
        public Guid Id { get; set; }
        public Guid DynamicFormId { get; set; }
        public string OwnerType { get; set; } = string.Empty;
        public Guid OwnerId { get; set; }
        public Dictionary<string, string?> Data { get; set; } = [];
        /// <summary>Attached-file counts keyed by Attachment field name (each field is a separate pool).</summary>
        public Dictionary<string, int> DocumentCounts { get; set; } = [];
    }

    public class SaveDynamicFormRecordDto
    {
        public Guid? Id { get; set; }
        public Guid DynamicFormId { get; set; }
        public string OwnerType { get; set; } = string.Empty;
        public Guid OwnerId { get; set; }
        public Dictionary<string, string?>? Data { get; set; }
    }

    // ---- Validators ---------------------------------------------------------
    public class SaveDynamicFormFieldDtoValidator : AbstractValidator<SaveDynamicFormFieldDto>
    {
        public SaveDynamicFormFieldDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100)
                .Matches("^[a-zA-Z][a-zA-Z0-9_]*$").WithMessage("Field name must be a letter followed by letters, digits or underscores.");
            RuleFor(x => x.Label).NotEmpty().MaximumLength(200);
            RuleFor(x => x.DataType).NotEmpty()
                .Must(v => Enum.TryParse<EmployeeFieldDataType>(v, out _))
                .WithMessage("DataType must be one of: Text, Number, Date, Boolean, Select.");
            RuleFor(x => x.Options).NotEmpty()
                .When(x => x.DataType == nameof(EmployeeFieldDataType.Select))
                .WithMessage("Select fields require comma-separated options.");
        }
    }

    public class SaveDynamicFormDtoValidator : AbstractValidator<SaveDynamicFormDto>
    {
        public SaveDynamicFormDtoValidator()
        {
            RuleFor(x => x.Module).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100)
                .Matches("^[a-zA-Z][a-zA-Z0-9_]*$").WithMessage("Form name must be a letter followed by letters, digits or underscores.");
            RuleFor(x => x.Label).NotEmpty().MaximumLength(200);
            RuleForEach(x => x.Fields).SetValidator(new SaveDynamicFormFieldDtoValidator());
        }
    }

    public class SaveDynamicFormRecordDtoValidator : AbstractValidator<SaveDynamicFormRecordDto>
    {
        public SaveDynamicFormRecordDtoValidator()
        {
            RuleFor(x => x.DynamicFormId).NotEmpty();
            RuleFor(x => x.OwnerType).NotEmpty().MaximumLength(30);
            RuleFor(x => x.OwnerId).NotEmpty();
        }
    }

    // ---- Service ------------------------------------------------------------
    /// <summary>
    /// Reusable, module-agnostic engine for user-defined dynamic forms/tabs (see <see cref="DynamicForm"/>).
    /// Metadata (forms + fields) drives rendering; each record's values are one JSON document validated
    /// against the form's active fields, so reads are a single indexed query with no EAV pivot.
    /// </summary>
    public interface IDynamicFormService
    {
        Task<List<DynamicFormDto>> GetActiveFormsAsync(string module);
        Task<PaginatedResponse<DynamicFormDto>> GetAllFormsAsync(GetAllRequest request);
        Task<DynamicFormDto> GetFormByIdAsync(Guid id);
        Task<Guid> SaveFormAsync(SaveDynamicFormDto dto);
        Task DeleteFormAsync(Guid id);

        Task<PaginatedResponse<DynamicFormRecordDto>> GetRecordsAsync(Guid formId, string ownerType, Guid ownerId, GetAllRequest request);
        Task<Guid> SaveRecordAsync(SaveDynamicFormRecordDto dto);
        Task DeleteRecordAsync(Guid id);
    }

    public class DynamicFormService(
        IRepository<DynamicForm> formRepository,
        IRepository<DynamicFormField> fieldRepository,
        IRepository<DynamicFormRecord> recordRepository,
        IRepository<EmployeeDocument> documentRepository,
        IValidator<SaveDynamicFormDto> formValidator,
        IValidator<SaveDynamicFormRecordDto> recordValidator,
        ILogger<DynamicFormService> logger) : IDynamicFormService
    {
        // ---- Metadata -------------------------------------------------------
        public async Task<List<DynamicFormDto>> GetActiveFormsAsync(string module)
        {
            var forms = await formRepository.GetAll()
                .Where(f => f.Module == module && f.IsActive)
                .Include(f => f.Fields)
                .OrderBy(f => f.SortOrder).ThenBy(f => f.Label)
                .AsNoTracking()
                .ToListAsync();
            return forms.Select(f => MapForm(f, activeFieldsOnly: true)).ToList();
        }

        public async Task<PaginatedResponse<DynamicFormDto>> GetAllFormsAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = formRepository.GetAll().Include(f => f.Fields).AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.Module))
                query = query.Where(f => f.Module == request.Module);
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(f => f.Name.Contains(term) || f.Label.Contains(term));
            }
            if (!string.IsNullOrWhiteSpace(request.Status) && bool.TryParse(request.Status, out var active))
                query = query.Where(f => f.IsActive == active);

            var total = await query.CountAsync();
            var forms = await query
                .OrderBy(f => f.SortOrder).ThenBy(f => f.Label)
                .Skip(skip).Take(take)
                .ToListAsync();

            return new PaginatedResponse<DynamicFormDto>
            {
                Total = total,
                Data = forms.Select(f => MapForm(f, activeFieldsOnly: false)).ToList()
            };
        }

        public async Task<DynamicFormDto> GetFormByIdAsync(Guid id)
        {
            var form = await formRepository.GetAll()
                .Include(f => f.Fields)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == id)
                ?? throw new NotFoundException(nameof(DynamicForm), id.ToString());
            return MapForm(form, activeFieldsOnly: false);
        }

        // ---- Form definition CRUD ------------------------------------------
        public async Task<Guid> SaveFormAsync(SaveDynamicFormDto dto)
        {
            var validation = await formValidator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            // Names are unique per (tenant, module).
            if (await formRepository.GetAll().AnyAsync(f => f.Module == dto.Module && f.Name == dto.Name && f.Id != (dto.Id ?? Guid.Empty)))
                throw new DuplicateException(nameof(DynamicForm), nameof(dto.Name), dto.Name);

            var specs = dto.Fields.Select(f => new DynamicFormFieldSpec(
                f.Name, f.Label, Enum.Parse<EmployeeFieldDataType>(f.DataType), f.Options,
                f.IsRequired, f.IsActive, f.SortOrder, f.ShowInList)).ToList();

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await formRepository.GetAll()
                        .Include(f => f.Fields)
                        .FirstOrDefaultAsync(f => f.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(DynamicForm), dto.Id.Value.ToString());

                // Old field rows are replaced wholesale — remove them so stale columns don't linger.
                foreach (var old in entity.Fields.ToList())
                    fieldRepository.Delete(old);

                entity.Update(dto.Label, dto.Description, dto.Icon, dto.IsActive, dto.SortOrder);
                entity.SetFields(specs);
                StampFieldTenant(entity);
                // Replacement fields are new rows: mark them Added explicitly, otherwise
                // context.Update(root) treats the app-generated keys as existing (Modified).
                foreach (var field in entity.Fields)
                    await fieldRepository.AddAsync(field);
                formRepository.UpdateAsync(entity);
                await formRepository.SaveChangesAsync();
                logger.LogInformation("Updated DynamicForm {Id}", entity.Id);
                return entity.Id;
            }

            var created = DynamicForm.Create(dto.Module, dto.Name, dto.Label, dto.Description, dto.Icon, dto.IsActive, dto.SortOrder);
            created.SetFields(specs);
            await formRepository.AddAsync(created);   // stamps the root's TenantId
            StampFieldTenant(created);
            await formRepository.SaveChangesAsync();
            logger.LogInformation("Created DynamicForm {Id} ({Module}/{Name})", created.Id, created.Module, created.Name);
            return created.Id;
        }

        public async Task DeleteFormAsync(Guid id)
        {
            var entity = await formRepository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(DynamicForm), id.ToString());

            if (await recordRepository.GetAll().AnyAsync(r => r.DynamicFormId == id))
                throw new ValidationException(nameof(id),
                    "This form has stored records. Deactivate it instead of deleting to preserve data.");

            formRepository.Delete(entity);   // fields cascade
            await formRepository.SaveChangesAsync();
            logger.LogInformation("Deleted DynamicForm {Id}", id);
        }

        // ---- Record CRUD ----------------------------------------------------
        public async Task<PaginatedResponse<DynamicFormRecordDto>> GetRecordsAsync(
            Guid formId, string ownerType, Guid ownerId, GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 25;

            var query = recordRepository.GetAll()
                .Where(r => r.DynamicFormId == formId && r.OwnerType == ownerType && r.OwnerId == ownerId);

            var total = await query.CountAsync();

            // Only the requested page is fetched + JSON-parsed. The index
            // (DynamicFormId, OwnerType, OwnerId, CreatedAt) supports both the seek and the ordered
            // pagination, so this stays flat as a form's per-owner record count grows.
            var rows = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip(skip).Take(take)
                .Select(r => new { r.Id, r.DynamicFormId, r.OwnerType, r.OwnerId, r.Data })
                .AsNoTracking()
                .ToListAsync();

            // Per-field attachment counts for the whole page in ONE grouped query (no N+1). Each
            // Attachment field is a separate pool, keyed by OwnerField (the field name).
            var countsByRecord = new Dictionary<Guid, Dictionary<string, int>>();
            if (rows.Count > 0)
            {
                var recordIds = rows.Select(r => r.Id).ToList();
                var counts = await documentRepository.GetAll()
                    .Where(d => d.OwnerType == EmployeeDocumentOwner.DynamicFormRecord
                                && d.OwnerField != null && recordIds.Contains(d.OwnerId))
                    .GroupBy(d => new { d.OwnerId, d.OwnerField })
                    .Select(g => new { g.Key.OwnerId, g.Key.OwnerField, Count = g.Count() })
                    .ToListAsync();
                countsByRecord = counts
                    .GroupBy(c => c.OwnerId)
                    .ToDictionary(g => g.Key, g => g.ToDictionary(x => x.OwnerField!, x => x.Count));
            }

            return new PaginatedResponse<DynamicFormRecordDto>
            {
                Total = total,
                Data = rows.Select(r => new DynamicFormRecordDto
                {
                    Id = r.Id,
                    DynamicFormId = r.DynamicFormId,
                    OwnerType = r.OwnerType,
                    OwnerId = r.OwnerId,
                    Data = Deserialize(r.Data),
                    DocumentCounts = countsByRecord.TryGetValue(r.Id, out var m) ? m : []
                }).ToList()
            };
        }

        public async Task<Guid> SaveRecordAsync(SaveDynamicFormRecordDto dto)
        {
            var validation = await recordValidator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            // Attachment fields hold files (in EmployeeDocument), not a JSON value — exclude them here.
            var fields = await fieldRepository.GetAll()
                .Where(f => f.DynamicFormId == dto.DynamicFormId && f.IsActive && f.DataType != EmployeeFieldDataType.Attachment)
                .Select(f => new { f.Name, f.Label, f.IsRequired })
                .ToListAsync();
            if (fields.Count == 0 && !await formRepository.GetAll().AnyAsync(f => f.Id == dto.DynamicFormId))
                throw new NotFoundException(nameof(DynamicForm), dto.DynamicFormId.ToString());

            var submitted = dto.Data ?? [];
            var names = fields.Select(f => f.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

            var unknown = submitted.Keys.Where(k => !names.Contains(k)).ToList();
            if (unknown.Count > 0)
                throw new ValidationException("data", $"Unknown field(s): {string.Join(", ", unknown)}");
            foreach (var f in fields.Where(f => f.IsRequired))
            {
                submitted.TryGetValue(f.Name, out var v);
                if (string.IsNullOrWhiteSpace(v))
                    throw new ValidationException(f.Name, $"'{f.Label}' is required.");
            }

            // Persist only known fields (drop any stray keys), as a compact JSON document.
            var clean = fields.ToDictionary(f => f.Name, f => submitted.TryGetValue(f.Name, out var v) ? v : null);
            var json = JsonSerializer.Serialize(clean);

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await recordRepository.GetAll().FirstOrDefaultAsync(r => r.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(DynamicFormRecord), dto.Id.Value.ToString());
                entity.SetData(json);
                recordRepository.UpdateAsync(entity);
                await recordRepository.SaveChangesAsync();
                logger.LogInformation("Updated DynamicFormRecord {Id}", entity.Id);
                return entity.Id;
            }

            var created = DynamicFormRecord.Create(dto.DynamicFormId, dto.OwnerType, dto.OwnerId, json);
            await recordRepository.AddAsync(created);
            await recordRepository.SaveChangesAsync();
            logger.LogInformation("Created DynamicFormRecord {Id} (form {FormId})", created.Id, dto.DynamicFormId);
            return created.Id;
        }

        public async Task DeleteRecordAsync(Guid id)
        {
            var entity = await recordRepository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(DynamicFormRecord), id.ToString());
            // Attached files cascade with the record (polymorphic owner, no FK) — same as education/experience.
            await DocumentStorage.DeleteForOwnerAsync(documentRepository, EmployeeDocumentOwner.DynamicFormRecord, id);
            recordRepository.Delete(entity);
            await recordRepository.SaveChangesAsync();
            logger.LogInformation("Deleted DynamicFormRecord {Id}", id);
        }

        // ---- Helpers --------------------------------------------------------
        private static DynamicFormDto MapForm(DynamicForm f, bool activeFieldsOnly) => new()
        {
            Id = f.Id,
            Module = f.Module,
            Name = f.Name,
            Label = f.Label,
            Description = f.Description,
            Icon = f.Icon,
            IsActive = f.IsActive,
            SortOrder = f.SortOrder,
            Fields = f.Fields
                .Where(x => !activeFieldsOnly || x.IsActive)
                .OrderBy(x => x.SortOrder)
                .Select(x => new DynamicFormFieldDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Label = x.Label,
                    DataType = x.DataType.ToString(),
                    Options = x.Options,
                    IsRequired = x.IsRequired,
                    IsActive = x.IsActive,
                    SortOrder = x.SortOrder,
                    ShowInList = x.ShowInList
                })
                .ToList()
        };

        private static Dictionary<string, string?> Deserialize(string? data)
        {
            if (string.IsNullOrWhiteSpace(data)) return [];
            try { return JsonSerializer.Deserialize<Dictionary<string, string?>>(data) ?? []; }
            catch { return []; }
        }

        /// <summary>The repository stamps only aggregate roots — cascade-inserted fields copy it here.</summary>
        private static void StampFieldTenant(DynamicForm form)
        {
            foreach (var field in form.Fields)
                if (string.IsNullOrEmpty(field.TenantId))
                    field.TenantId = form.TenantId;
        }
    }
}
