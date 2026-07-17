using System.Text.Json;
using System.Text.RegularExpressions;
using CyberErp.Hrms.App.Common.DTOs;
using CyberErp.Hrms.App.Common.Exceptions;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = CyberErp.Hrms.App.Common.Exceptions.ValidationException;

namespace CyberErp.Hrms.App.Features.Core.Reports
{
    // ---- DTOs ---------------------------------------------------------------
    public class SavedFilterItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class ReportCatalogItemDto
    {
        public Guid Id { get; set; }
        public string ReportKey { get; set; } = string.Empty;
        public string ReportName { get; set; } = string.Empty;
        public string? Description { get; set; }
        /// <summary>Saved filter sets — the report's "children" in the reference menu tree.</summary>
        public List<SavedFilterItemDto> SavedFilters { get; set; } = [];
    }

    public class ReportCatalogGroupDto
    {
        public string Grouping { get; set; } = string.Empty;
        public List<ReportCatalogItemDto> Reports { get; set; } = [];
    }

    public class ReportLookupOptionDto
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class ReportFieldDto
    {
        public string Field { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string DataType { get; set; } = nameof(ReportFieldDataType.Text);
        public int FieldOrder { get; set; }
        public string? DependencyField { get; set; }
        /// <summary>'#' in the field name declares a From/To range pair ('#'→'1'/'2').</summary>
        public bool IsRange { get; set; }
        /// <summary>Preloaded options for Select/MultiSelect/Radio without a dependency.</summary>
        public List<ReportLookupOptionDto>? Options { get; set; }
    }

    public class ReportOutputColumnDto
    {
        public string Field { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public int FieldOrder { get; set; }
    }

    public class ReportSchemaDto
    {
        public Guid Id { get; set; }
        public string ReportKey { get; set; } = string.Empty;
        public string ReportName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<ReportFieldDto> Fields { get; set; } = [];
        /// <summary>Selectable output columns (empty = the SP always returns its full set).</summary>
        public List<ReportOutputColumnDto> OutputColumns { get; set; } = [];
        /// <summary>Pivot / grouping layout (reference GridConfig). Null = a flat report.</summary>
        public ReportGroupingDto? Grouping { get; set; }
    }

    /// <summary>Pivot / grouping layout exposed to the viewer (reference GridConfig / ReportGridLayoutOptions).</summary>
    public class ReportGroupingDto
    {
        /// <summary>Report is a pivot report OR lets the user group it (default groupBy set OR allowUserCustomize).</summary>
        public bool SupportsGrouping { get; set; }
        /// <summary>The user may change the grouping at run time (the "Grouping" popup).</summary>
        public bool AllowUserCustomize { get; set; }
        public int MaxGroupLevels { get; set; } = 3;
        /// <summary>Show a per-group summary (count) header.</summary>
        public bool ShowGroupSummary { get; set; } = true;
        /// <summary>Default group-by columns, in level order.</summary>
        public List<string> GroupBy { get; set; } = [];
        /// <summary>Columns the user may group by (the report's output columns).</summary>
        public List<ReportOutputColumnDto> GroupableFields { get; set; } = [];
    }

    /// <summary>
    /// One customized output column (reference pReportFieldOutput packed as Field■Label■order■sortOrder):
    /// <see cref="Order"/> is the display position (drag-reorder), <see cref="SortOrder"/> is the ORDER BY
    /// priority 0–9 (0 = not sorted), <see cref="Label"/> overrides the column header. Only the columns the
    /// user left checked are sent (in display order).
    /// </summary>
    public class ReportOutputFieldDto
    {
        public string Field { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public int Order { get; set; }
        public int SortOrder { get; set; }
    }

    public class GenerateReportDto
    {
        public string ReportKey { get; set; } = string.Empty;
        /// <summary>Field → value (range fields post Field1/Field2; multiselects post comma lists).</summary>
        public Dictionary<string, string?> Values { get; set; } = [];
        /// <summary>Chosen output columns with per-column order/sort/label (null/empty = SP default set).</summary>
        public List<ReportOutputFieldDto>? OutputFields { get; set; }
    }

    public class SaveReportFilterDto
    {
        public string ReportKey { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, string?> Values { get; set; } = [];
        public List<string>? OutputFields { get; set; }
    }

    public class ReportFilterDto : SaveReportFilterDto
    {
        public Guid Id { get; set; }
    }

    public class ReportRunDto
    {
        public string ReportKey { get; set; } = string.Empty;
        public string? RanBy { get; set; }
        public DateTime RanAt { get; set; }
        public int RowCount { get; set; }
        public int DurationMs { get; set; }
    }

    public class ReportColumnDto
    {
        public string Field { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Type { get; set; } = "string";
        public int? Width { get; set; }
        public string? LinkPage { get; set; }
        public string? LinkPageValue { get; set; }
    }

    public class ReportResultDto
    {
        public string ReportKey { get; set; } = string.Empty;
        public string ReportName { get; set; } = string.Empty;
        public List<ReportColumnDto> Columns { get; set; } = [];
        public List<Dictionary<string, object?>> Rows { get; set; } = [];
        public int Total { get; set; }
        /// <summary>PIVOT: per-group subtotals from a grouping SP's 3rd result set (null = a flat report).</summary>
        public List<Dictionary<string, object?>>? Summaries { get; set; }
    }

    // Admin registry DTOs
    public class SaveReportFieldDto
    {
        public string Field { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string DataType { get; set; } = nameof(ReportFieldDataType.Text);
        public int? FieldOrder { get; set; }
        public string? DependencyField { get; set; }
    }

    public class SaveReportOutputDto
    {
        public string Field { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public int? FieldOrder { get; set; }
    }

    public class SaveReportDto
    {
        public Guid? Id { get; set; }
        public string ReportKey { get; set; } = string.Empty;
        public string ReportName { get; set; } = string.Empty;
        public string ReportGrouping { get; set; } = "General";
        public string StoredProc { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        /// <summary>Pivot / grouping layout JSON (reference GridConfig). Null/empty = a flat report.</summary>
        public string? GridConfig { get; set; }
        public List<SaveReportFieldDto> Fields { get; set; } = [];
        /// <summary>Selectable output columns offered in the viewer's column chooser.</summary>
        public List<SaveReportOutputDto> FieldOutputs { get; set; } = [];
        /// <summary>Restrict-to role ids (reference ReportClientRestriction). Empty = everyone.</summary>
        public List<Guid> RoleIds { get; set; } = [];
    }

    public class ReportDto : SaveReportDto
    {
        public new Guid Id { get; set; }
    }

    public partial class SaveReportDtoValidator : AbstractValidator<SaveReportDto>
    {
        [GeneratedRegex(@"^[A-Za-z0-9_]+$")]
        private static partial Regex KeyRegex();
        // Optionally schema-qualified / bracketed procedure identifier — nothing else.
        [GeneratedRegex(@"^\[?[A-Za-z0-9_]+\]?(\.\[?[A-Za-z0-9_]+\]?)?$")]
        private static partial Regex ProcRegex();

        public SaveReportDtoValidator()
        {
            RuleFor(x => x.ReportKey).NotEmpty().MaximumLength(100)
                .Must(v => KeyRegex().IsMatch(v ?? string.Empty))
                .WithMessage("ReportKey may contain only letters, digits and underscores.");
            RuleFor(x => x.ReportName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.ReportGrouping).MaximumLength(150);
            RuleFor(x => x.StoredProc).NotEmpty().MaximumLength(200)
                .Must(v => ProcRegex().IsMatch(v ?? string.Empty))
                .WithMessage("StoredProc must be a plain (optionally schema-qualified) procedure name.");
            RuleFor(x => x.Description).MaximumLength(1000);
            RuleForEach(x => x.Fields).ChildRules(f =>
            {
                f.RuleFor(y => y.Field).NotEmpty().MaximumLength(100);
                f.RuleFor(y => y.Label).MaximumLength(200);
                f.RuleFor(y => y.DataType).NotEmpty()
                    .Must(v => Enum.TryParse<ReportFieldDataType>(v, true, out _))
                    .WithMessage("DataType must be Text, Number, Currency, Check, Date, Select, MultiSelect or Radio.");
            });
        }
    }

    /// <summary>Role-restriction gate (reference ReportClientRestriction / _x_ReportList).</summary>
    public interface IReportAccessGuard
    {
        /// <summary>Report ids the current user may NOT see (restricted to roles they lack).</summary>
        Task<HashSet<Guid>> GetHiddenReportIdsAsync();
        /// <summary>404s when the report is restricted away from the current user.</summary>
        Task EnsureAccessAsync(Guid reportId, string reportKey);
    }

    public class ReportAccessGuard(
        IRepository<ReportRestriction> restrictions,
        IRepository<UserRole> userRoles,
        Common.Services.ICurrentUserService currentUser) : IReportAccessGuard
    {
        private async Task<HashSet<Guid>> MyRoleIdsAsync()
        {
            var userId = currentUser.GetCurrentUserId();
            if (userId is null) return [];
            return (await userRoles.GetAll().Where(u => u.UserId == userId.Value)
                .Select(u => u.RoleId).ToListAsync()).ToHashSet();
        }

        public async Task<HashSet<Guid>> GetHiddenReportIdsAsync()
        {
            var rows = await restrictions.GetAll()
                .Select(r => new { r.ReportId, r.RoleId }).ToListAsync();
            if (rows.Count == 0) return [];
            var mine = await MyRoleIdsAsync();
            return rows.GroupBy(r => r.ReportId)
                .Where(g => !g.Any(r => mine.Contains(r.RoleId)))
                .Select(g => g.Key).ToHashSet();
        }

        public async Task EnsureAccessAsync(Guid reportId, string reportKey)
        {
            var roleIds = await restrictions.GetAll().Where(r => r.ReportId == reportId)
                .Select(r => r.RoleId).ToListAsync();
            if (roleIds.Count == 0) return;
            var mine = await MyRoleIdsAsync();
            if (!roleIds.Any(mine.Contains))
                throw new NotFoundException(nameof(Report), reportKey); // don't leak existence
        }
    }

    // ---- Interfaces ---------------------------------------------------------
    public interface IGetReportCatalog { Task<List<ReportCatalogGroupDto>> GetAsync(); }
    public interface IGetReportSchema { Task<ReportSchemaDto> GetAsync(string reportKey); }
    public interface IGetReportFieldValues { Task<List<ReportLookupOptionDto>> GetAsync(string reportKey, string field, string? dependency, string? search); }
    public interface IGenerateReport { Task<ReportResultDto> GenerateAsync(GenerateReportDto dto); }
    public interface ISaveReport { Task<Guid> SaveAsync(SaveReportDto dto); }
    public interface IDeleteReport { Task DeleteAsync(Guid id); }
    public interface IGetReportById { Task<ReportDto> GetAsync(Guid id); }
    public interface IGetAllReports { Task<PaginatedResponse<ReportDto>> GetAsync(GetAllRequest request); }
    public class SetReportRestrictionsDto
    {
        public string ReportKey { get; set; } = string.Empty;
        public List<Guid> RoleIds { get; set; } = [];
    }

    /// <summary>The "Restricted Roles" popup (reference _x_ReportRestriction): replaces restrict-to roles.</summary>
    public interface ISetReportRestrictions { Task SetAsync(SetReportRestrictionsDto dto); }

    public interface ISaveReportFilter { Task<Guid> SaveAsync(SaveReportFilterDto dto); }
    public interface IGetReportFilter { Task<ReportFilterDto> GetAsync(Guid id); }
    public interface IDeleteReportFilter { Task DeleteAsync(Guid id); }
    public interface IGetReportHistory { Task<List<ReportRunDto>> GetAsync(string? reportKey); }

    // ---- Viewer: catalog / schema / lookups / generate -----------------------
    public class GetReportCatalog(
        IRepository<Report> repository,
        IRepository<SavedReportFilter> savedFilters,
        IReportAccessGuard accessGuard) : IGetReportCatalog
    {
        public async Task<List<ReportCatalogGroupDto>> GetAsync()
        {
            var hidden = await accessGuard.GetHiddenReportIdsAsync();
            var rows = await repository.GetAll()
                .Where(r => r.IsActive)
                .OrderBy(r => r.ReportGrouping).ThenBy(r => r.SortOrder).ThenBy(r => r.ReportName)
                .Select(r => new { r.Id, r.ReportKey, r.ReportName, r.ReportGrouping, r.Description })
                .ToListAsync();

            rows = rows.Where(r => !hidden.Contains(r.Id)).ToList();

            // Saved filter sets render as the report's children (reference menu tree behavior).
            var reportIds = rows.Select(r => r.Id).ToList();
            var filters = await savedFilters.GetAll()
                .Where(f => reportIds.Contains(f.ReportId))
                .OrderBy(f => f.Name)
                .Select(f => new { f.Id, f.ReportId, f.Name })
                .ToListAsync();

            return rows.GroupBy(r => r.ReportGrouping)
                .Select(g => new ReportCatalogGroupDto
                {
                    Grouping = g.Key,
                    Reports = g.Select(r => new ReportCatalogItemDto
                    {
                        Id = r.Id, ReportKey = r.ReportKey, ReportName = r.ReportName, Description = r.Description,
                        SavedFilters = filters.Where(f => f.ReportId == r.Id)
                            .Select(f => new SavedFilterItemDto { Id = f.Id, Name = f.Name }).ToList()
                    }).ToList()
                })
                .ToList();
        }
    }

    public class GetReportSchema(IRepository<Report> repository, IReportExecutor executor, IReportAccessGuard accessGuard) : IGetReportSchema
    {
        public async Task<ReportSchemaDto> GetAsync(string reportKey)
        {
            var report = await repository.GetAll()
                .Include(r => r.Fields)
                .Include(r => r.FieldOutputs)
                .FirstOrDefaultAsync(r => r.ReportKey == reportKey && r.IsActive)
                ?? throw new NotFoundException(nameof(Report), reportKey);
            await accessGuard.EnsureAccessAsync(report.Id, reportKey);

            var schema = new ReportSchemaDto
            {
                Id = report.Id,
                ReportKey = report.ReportKey,
                ReportName = report.ReportName,
                Description = report.Description,
                OutputColumns = report.FieldOutputs.OrderBy(o => o.FieldOrder)
                    .Select(o => new ReportOutputColumnDto { Field = o.Field, Label = o.Label, FieldOrder = o.FieldOrder })
                    .ToList(),
                Fields = report.Fields.OrderBy(f => f.FieldOrder).Select(f => new ReportFieldDto
                {
                    Field = f.Field,
                    Label = f.Label,
                    DataType = f.DataType.ToString(),
                    FieldOrder = f.FieldOrder,
                    DependencyField = f.DependencyField,
                    IsRange = f.Field.Contains('#')
                }).ToList()
            };

            // Preload options for choice fields WITHOUT a dependency (cascading children load on demand),
            // mirroring the reference GetReportFields behavior.
            foreach (var f in schema.Fields.Where(f =>
                         f.DependencyField is null &&
                         f.DataType is nameof(ReportFieldDataType.Select)
                             or nameof(ReportFieldDataType.MultiSelect)
                             or nameof(ReportFieldDataType.Radio)))
            {
                var options = await executor.GetFieldValuesAsync(report.ReportKey, f.Field, null, null);
                f.Options = options.Select(o => new ReportLookupOptionDto { Value = o.Value, Label = o.Label }).ToList();
            }

            schema.Grouping = ParseGridConfig(report.GridConfig, schema.OutputColumns);
            return schema;
        }

        /// <summary>Parse the report's GridConfig JSON into the grouping layout the viewer needs.</summary>
        internal static ReportGroupingDto? ParseGridConfig(string? gridConfig, List<ReportOutputColumnDto> outputColumns)
        {
            if (string.IsNullOrWhiteSpace(gridConfig)) return null;
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(gridConfig);
                var root = doc.RootElement;
                var groupBy = root.TryGetProperty("groupBy", out var gb) && gb.ValueKind == System.Text.Json.JsonValueKind.Array
                    ? gb.EnumerateArray().Select(e => e.GetString() ?? "").Where(s => s.Length > 0).ToList() : [];
                var allowCustomize = root.TryGetProperty("allowUserCustomize", out var ac) && ac.ValueKind == System.Text.Json.JsonValueKind.True;
                var maxLevels = root.TryGetProperty("maxGroupLevels", out var ml) && ml.TryGetInt32(out var m) && m > 0 ? m : 3;
                var showSummary = !root.TryGetProperty("showGroupSummary", out var ss) || ss.ValueKind != System.Text.Json.JsonValueKind.False;
                if (groupBy.Count == 0 && !allowCustomize) return null;
                return new ReportGroupingDto
                {
                    GroupBy = groupBy,
                    AllowUserCustomize = allowCustomize,
                    MaxGroupLevels = maxLevels,
                    ShowGroupSummary = showSummary,
                    SupportsGrouping = true,
                    GroupableFields = outputColumns,
                };
            }
            catch { return null; }
        }
    }

    public class GetReportFieldValues(IRepository<Report> repository, IReportExecutor executor, IReportAccessGuard accessGuard) : IGetReportFieldValues
    {
        public async Task<List<ReportLookupOptionDto>> GetAsync(string reportKey, string field, string? dependency, string? search)
        {
            var restrictedId = await repository.GetAll().Where(r => r.ReportKey == reportKey)
                .Select(r => (Guid?)r.Id).FirstOrDefaultAsync();
            if (restrictedId.HasValue) await accessGuard.EnsureAccessAsync(restrictedId.Value, reportKey);
            // The field must be registered metadata — arbitrary lookups can't be probed. The reserved
            // '@DynamicDate' key is engine-provided (the relative-date catalog), so it bypasses that check.
            if (field != DynamicDate.FieldKey)
            {
                var known = await repository.GetAll()
                    .Where(r => r.ReportKey == reportKey && r.IsActive)
                    .SelectMany(r => r.Fields)
                    .AnyAsync(f => f.Field == field);
                if (!known) throw new NotFoundException(nameof(ReportField), $"{reportKey}/{field}");
            }

            var options = await executor.GetFieldValuesAsync(reportKey, field, dependency, search);
            return options.Select(o => new ReportLookupOptionDto { Value = o.Value, Label = o.Label }).ToList();
        }
    }

    public class GenerateReport(
        IRepository<Report> repository,
        IReportScheduleStore store,
        IReportAccessGuard accessGuard,
        IReportExecutor executor,
        Common.Services.ICurrentUserService currentUser,
        ILogger<GenerateReport> logger) : IGenerateReport
    {
        public async Task<ReportResultDto> GenerateAsync(GenerateReportDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ReportKey))
                throw new ValidationException("reportKey", "Report key is required.");

            // The stored procedure name comes from the TENANT-SCOPED registry row — never from the
            // client payload (the reference echoed it through a hidden field; we don't).
            var report = await repository.GetAll()
                .FirstOrDefaultAsync(r => r.ReportKey == dto.ReportKey && r.IsActive)
                ?? throw new NotFoundException(nameof(Report), dto.ReportKey);
            await accessGuard.EnsureAccessAsync(report.Id, report.ReportKey);

            // All user filter values ride in ONE bound JSON parameter, parsed inside the SP
            // (the SQL Server equivalent of the reference's single delimited pCriteria string);
            // the chosen output columns ride in @OutputFields (the pReportFieldOutput port).
            var criteriaJson = JsonSerializer.Serialize(dto.Values ?? []);
            var outputJson = dto.OutputFields is { Count: > 0 } ? JsonSerializer.Serialize(dto.OutputFields) : null;

            try
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var result = await executor.ExecuteAsync(report.StoredProc, report.ReportKey, criteriaJson, outputJson);
                sw.Stop();

                // Run history (reference _x_ReportGenerateSendToHistory) — best effort, never blocks the report.
                try
                {
                    await store.SendToHistoryAsync(string.Empty,
                        report.ReportKey, isScheduled: false, criteriaJson, outputJson, result.Rows.Count,
                        (int)(sw.ElapsedMilliseconds / 1000), currentUser.GetCurrentUserName(), null);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Report run history could not be recorded for {Key}", report.ReportKey);
                }

                logger.LogInformation("Report {Key} generated: {Rows} row(s) in {Ms}ms",
                    report.ReportKey, result.Rows.Count, sw.ElapsedMilliseconds);
                return new ReportResultDto
                {
                    ReportKey = report.ReportKey,
                    ReportName = report.ReportName,
                    Columns = result.Columns.Select(c => new ReportColumnDto
                    {
                        Field = c.Field, Label = c.Label, Type = c.Type, Width = c.Width,
                        LinkPage = c.LinkPage, LinkPageValue = c.LinkPageValue
                    }).ToList(),
                    Rows = result.Rows.ToList(),
                    Total = result.Rows.Count,
                    Summaries = result.Summaries?.ToList()
                };
            }
            catch (TimeoutException)
            {
                throw new ValidationException("report",
                    "The report timed out. Please narrow your criteria (e.g. a shorter date range) and try again.");
            }
        }
    }

    // ---- Admin registry CRUD --------------------------------------------------
    public class SaveReport(
        IRepository<Report> repository,
        IRepository<ReportField> fieldRepository,
        IRepository<ReportFieldOutput> outputRepository,
        IRepository<ReportRestriction> restrictionRepository,
        IRepository<Role> roleRepository,
        IReportExecutor executor,
        IValidator<SaveReportDto> validator,
        ILogger<SaveReport> logger) : ISaveReport
    {
        public async Task<Guid> SaveAsync(SaveReportDto dto)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid) throw new ValidationException(validation.ToDictionary());

            if (await repository.GetAll().AnyAsync(r => r.ReportKey == dto.ReportKey && r.Id != dto.Id))
                throw new DuplicateException(nameof(Report), nameof(dto.ReportKey), dto.ReportKey);

            // Fail at save time (not first run) when the named procedure doesn't exist: probing the
            // lookup SP validates connectivity, and existence is checked inside the executor.
            await ValidateStoredProcAsync(dto.StoredProc);

            var specs = dto.Fields.Select(f => new ReportFieldSpec(
                f.Field, f.Label, Enum.Parse<ReportFieldDataType>(f.DataType, true), f.FieldOrder, f.DependencyField)).ToList();
            var outputSpecs = dto.FieldOutputs.Select(o =>
                new ReportFieldOutputSpec(o.Field, o.Label, o.FieldOrder)).ToList();
            var roleNames = await roleRepository.GetAll().Where(r => dto.RoleIds.Contains(r.Id))
                .ToDictionaryAsync(r => r.Id, r => r.Name);
            var restrictionSpecs = dto.RoleIds.Distinct().Select(id => (id,
                roleNames.TryGetValue(id, out var n) ? n : throw new NotFoundException(nameof(Role), id.ToString()))).ToList();

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var entity = await repository.GetAll()
                        .Include(r => r.Fields)
                        .Include(r => r.FieldOutputs)
                        .Include(r => r.Restrictions)
                        .FirstOrDefaultAsync(x => x.Id == dto.Id.Value)
                    ?? throw new NotFoundException(nameof(Report), dto.Id.Value.ToString());
                entity.Update(dto.ReportKey, dto.ReportName, dto.ReportGrouping, dto.StoredProc,
                    dto.SortOrder, dto.Description, dto.IsActive, dto.GridConfig);
                entity.SetFields(specs);
                entity.SetFieldOutputs(outputSpecs);
                entity.SetRestrictions(restrictionSpecs);
                StampFieldTenant(entity);
                // Replacement children are NEW rows — mark them Added explicitly (app-generated keys
                // would otherwise be treated as Modified and fail the concurrency check).
                foreach (var field in entity.Fields)
                    await fieldRepository.AddAsync(field);
                foreach (var output in entity.FieldOutputs)
                    await outputRepository.AddAsync(output);
                foreach (var restriction in entity.Restrictions)
                    await restrictionRepository.AddAsync(restriction);
                repository.UpdateAsync(entity);
                await repository.SaveChangesAsync();
                logger.LogInformation("Updated Report {Id} ({Key})", entity.Id, entity.ReportKey);
                return entity.Id;
            }

            var created = Report.Create(dto.ReportKey, dto.ReportName, dto.ReportGrouping,
                dto.StoredProc, dto.SortOrder, dto.Description, dto.IsActive, dto.GridConfig);
            created.SetFields(specs);
            created.SetFieldOutputs(outputSpecs);
            created.SetRestrictions(restrictionSpecs);
            await repository.AddAsync(created);   // stamps the root's TenantId
            StampFieldTenant(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Created Report {Id} ({Key})", created.Id, created.ReportKey);
            return created.Id;
        }

        private async Task ValidateStoredProcAsync(string storedProc)
        {
            if (!await executor.StoredProcedureExistsAsync(storedProc))
                throw new ValidationException("storedProc",
                    $"Stored procedure '{storedProc}' does not exist in the database. Create it first.");
        }

        /// <summary>The repository stamps only aggregate roots — cascade-inserted children copy it here.</summary>
        private static void StampFieldTenant(Report report)
        {
            foreach (var field in report.Fields)
                if (string.IsNullOrEmpty(field.TenantId))
                    field.TenantId = report.TenantId;
            foreach (var output in report.FieldOutputs)
                if (string.IsNullOrEmpty(output.TenantId))
                    output.TenantId = report.TenantId;
            foreach (var restriction in report.Restrictions)
                if (string.IsNullOrEmpty(restriction.TenantId))
                    restriction.TenantId = report.TenantId;
        }
    }

    public class SetReportRestrictions(
        IRepository<Report> repository,
        IRepository<ReportRestriction> restrictionRepository,
        IRepository<Role> roleRepository) : ISetReportRestrictions
    {
        public async Task SetAsync(SetReportRestrictionsDto dto)
        {
            var report = await repository.GetAll().Include(r => r.Restrictions)
                .FirstOrDefaultAsync(r => r.ReportKey == dto.ReportKey)
                ?? throw new NotFoundException(nameof(Report), dto.ReportKey);
            var names = await roleRepository.GetAll().Where(r => dto.RoleIds.Contains(r.Id))
                .ToDictionaryAsync(r => r.Id, r => r.Name);
            report.SetRestrictions(dto.RoleIds.Distinct().Select(id => (id,
                names.TryGetValue(id, out var n) ? n : throw new NotFoundException(nameof(Role), id.ToString()))).ToList());
            foreach (var x in report.Restrictions)
            {
                if (string.IsNullOrEmpty(x.TenantId)) x.TenantId = report.TenantId;
                await restrictionRepository.AddAsync(x);
            }
            repository.UpdateAsync(report);
            await repository.SaveChangesAsync();
        }
    }

    // ---- Saved filter sets (the reference's non-scheduled "children") ---------
    public class SaveReportFilter(
        IRepository<Report> reports,
        IRepository<SavedReportFilter> repository,
        ILogger<SaveReportFilter> logger) : ISaveReportFilter
    {
        public async Task<Guid> SaveAsync(SaveReportFilterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ValidationException("name", "Give the saved filter a name.");
            var report = await reports.GetAll().FirstOrDefaultAsync(r => r.ReportKey == dto.ReportKey && r.IsActive)
                ?? throw new NotFoundException(nameof(Report), dto.ReportKey);

            var created = SavedReportFilter.Create(report.Id,
                dto.Name,
                JsonSerializer.Serialize(dto.Values ?? []),
                dto.OutputFields is { Count: > 0 } ? JsonSerializer.Serialize(dto.OutputFields) : null);
            await repository.AddAsync(created);
            await repository.SaveChangesAsync();
            logger.LogInformation("Saved report filter {Id} ({Name}) for {Key}", created.Id, created.Name, dto.ReportKey);
            return created.Id;
        }
    }

    public class GetReportFilter(
        IRepository<SavedReportFilter> repository,
        IRepository<Report> reports) : IGetReportFilter
    {
        public async Task<ReportFilterDto> GetAsync(Guid id)
        {
            var filter = await repository.GetAll().FirstOrDefaultAsync(f => f.Id == id)
                ?? throw new NotFoundException(nameof(SavedReportFilter), id.ToString());
            var reportKey = await reports.GetAll().Where(r => r.Id == filter.ReportId)
                .Select(r => r.ReportKey).FirstOrDefaultAsync() ?? string.Empty;

            return new ReportFilterDto
            {
                Id = filter.Id,
                ReportKey = reportKey,
                Name = filter.Name,
                Values = JsonSerializer.Deserialize<Dictionary<string, string?>>(filter.CriteriaJson) ?? [],
                OutputFields = filter.OutputFieldsJson is null
                    ? null
                    : JsonSerializer.Deserialize<List<string>>(filter.OutputFieldsJson)
            };
        }
    }

    public class DeleteReportFilter(IRepository<SavedReportFilter> repository) : IDeleteReportFilter
    {
        public async Task DeleteAsync(Guid id)
        {
            var filter = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(SavedReportFilter), id.ToString());
            repository.Delete(filter);
            await repository.SaveChangesAsync();
        }
    }

    // ---- Run history (reference ReportRun / _x_ReportHistorySearch) -----------
    public class GetReportHistory(IRepository<ReportRun> repository) : IGetReportHistory
    {
        public async Task<List<ReportRunDto>> GetAsync(string? reportKey)
        {
            var query = repository.GetAll();
            if (!string.IsNullOrWhiteSpace(reportKey))
                query = query.Where(r => r.ReportKey == reportKey);
            var rows = await query
                .OrderByDescending(r => r.CreatedAt)
                .Take(50)
                .Select(r => new { r.ReportKey, r.RanBy, r.CreatedAt, r.RowCount, r.DurationMs })
                .ToListAsync();
            return rows.Select(r => new ReportRunDto
            {
                ReportKey = r.ReportKey,
                RanBy = r.RanBy,
                RanAt = r.CreatedAt.ToDateTimeUtc(),
                RowCount = r.RowCount,
                DurationMs = r.DurationMs
            }).ToList();
        }
    }

    public class DeleteReport(
        IRepository<Report> repository,
        IReportScheduleStore store,
        ILogger<DeleteReport> logger) : IDeleteReport
    {
        public async Task DeleteAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Report), id.ToString());
            // reference _x_ReportDelete — removes the report and its dependent rows in one procedure.
            await store.DeleteReportAsync(entity.Id, string.Empty);
            logger.LogInformation("Deleted Report {Id}", id);
        }
    }

    /// <summary>Toggle a report's active flag (reference _x_ReportActivate).</summary>
    public interface ISetReportActive { Task SetAsync(Guid id, bool isActive); }

    public class SetReportActive(
        IRepository<Report> repository,
        IReportScheduleStore store) : ISetReportActive
    {
        public async Task SetAsync(Guid id, bool isActive)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(Report), id.ToString());
            await store.ActivateReportAsync(entity.Id, isActive, string.Empty);
        }
    }

    public class GetReportById(IRepository<Report> repository) : IGetReportById
    {
        public async Task<ReportDto> GetAsync(Guid id)
        {
            var report = await repository.GetAll().Include(r => r.Fields).Include(r => r.FieldOutputs)
                .Include(r => r.Restrictions)
                .FirstOrDefaultAsync(r => r.Id == id)
                ?? throw new NotFoundException(nameof(Report), id.ToString());
            return ReportMapper.ToDto(report);
        }
    }

    public class GetAllReports(IRepository<Report> repository) : IGetAllReports
    {
        public async Task<PaginatedResponse<ReportDto>> GetAsync(GetAllRequest request)
        {
            var skip = int.TryParse(request.Skip, out var s) ? s : 0;
            var take = int.TryParse(request.Take, out var t) ? t : 15;

            var query = repository.GetAll();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var term = request.SearchText.Trim();
                query = query.Where(r => r.ReportName.Contains(term) || r.ReportKey.Contains(term)
                    || r.ReportGrouping.Contains(term));
            }

            var total = await query.CountAsync();
            var data = await query
                .Include(r => r.Fields)
                .Include(r => r.FieldOutputs)
                .OrderBy(r => r.ReportGrouping).ThenBy(r => r.SortOrder).ThenBy(r => r.ReportName)
                .Skip(skip).Take(take)
                .ToListAsync();

            return new PaginatedResponse<ReportDto>
            {
                Total = total,
                Data = data.Select(ReportMapper.ToDto).ToList()
            };
        }
    }

    internal static class ReportMapper
    {
        public static ReportDto ToDto(Report r) => new()
        {
            Id = r.Id,
            ReportKey = r.ReportKey,
            ReportName = r.ReportName,
            ReportGrouping = r.ReportGrouping,
            StoredProc = r.StoredProc,
            SortOrder = r.SortOrder,
            Description = r.Description,
            IsActive = r.IsActive,
            GridConfig = r.GridConfig,
            Fields = r.Fields.OrderBy(f => f.FieldOrder).Select(f => new SaveReportFieldDto
            {
                Field = f.Field,
                Label = f.Label,
                DataType = f.DataType.ToString(),
                FieldOrder = f.FieldOrder,
                DependencyField = f.DependencyField
            }).ToList(),
            FieldOutputs = r.FieldOutputs.OrderBy(o => o.FieldOrder).Select(o => new SaveReportOutputDto
            {
                Field = o.Field,
                Label = o.Label,
                FieldOrder = o.FieldOrder
            }).ToList(),
            RoleIds = r.Restrictions.Select(x => x.RoleId).ToList()
        };
    }
}
