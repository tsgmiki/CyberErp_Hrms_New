using System.Text.Json.Serialization;
using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Input-control kind of a report parameter (ported from the reference engine's ReportField.DataType).</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReportFieldDataType
{
    Text = 0,
    Number = 1,
    Currency = 2,
    Check = 3,
    Date = 4,
    Select = 5,
    MultiSelect = 6,
    Radio = 7
}

/// <summary>
/// GENERIC report registry (ported from the reference APSmart report engine): a report is a data
/// row naming its own stored procedure — the engine itself is report-agnostic. The SP returns its
/// OWN schema (result set 1 = column metadata, result set 2 = rows), parameters are metadata rows
/// (<see cref="ReportField"/>), and dropdown options come from the master lookup procedure
/// (Core.hrms_ReportFieldValues). Filter values reach the SP as ONE bound @Criteria JSON parameter
/// parsed inside the procedure — no SQL is ever built from user input.
/// </summary>
public class Report : BaseEntity, IAggregateRoot, IAuditable
{
    /// <summary>Stable machine key (unique per tenant), e.g. "EmployeeDirectory".</summary>
    public string ReportKey { get; private set; } = string.Empty;
    public string ReportName { get; private set; } = string.Empty;
    /// <summary>Menu category the report is grouped under.</summary>
    public string ReportGrouping { get; private set; } = string.Empty;
    /// <summary>Name of the stored procedure that runs this report (schema-qualified).</summary>
    public string StoredProc { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;
    /// <summary>
    /// PIVOT / grouping layout (reference GridConfig JSON): defines whether the generated grid is a
    /// pivot report. Shape: <c>{ "groupBy": ["UnitName","EmploymentStatus"], "maxGroupLevels": 3,
    /// "allowUserCustomize": true, "showGroupSummary": true }</c>. Null/empty = a flat (normal) report.
    /// </summary>
    public string? GridConfig { get; private set; }

    private readonly List<ReportField> _fields = [];
    public IReadOnlyCollection<ReportField> Fields => _fields;

    private readonly List<ReportRestriction> _restrictions = [];
    /// <summary>Restrict-to roles (reference ReportClientRestriction). Empty = visible to everyone.</summary>
    public IReadOnlyCollection<ReportRestriction> Restrictions => _restrictions;

    /// <summary>Replaces the restrict-to roles.</summary>
    public void SetRestrictions(IEnumerable<(Guid RoleId, string RoleName)> roles)
    {
        _restrictions.Clear();
        foreach (var r in roles)
            _restrictions.Add(ReportRestriction.Create(Id, r.RoleId, r.RoleName));
    }

    private readonly List<ReportFieldOutput> _fieldOutputs = [];
    /// <summary>Selectable OUTPUT columns (reference ReportFieldOutput). Empty = SP returns everything.</summary>
    public IReadOnlyCollection<ReportFieldOutput> FieldOutputs => _fieldOutputs;

    private Report() : base() { }

    public static Report Create(string reportKey, string reportName, string reportGrouping,
        string storedProc, int sortOrder = 0, string? description = null, bool isActive = true, string? gridConfig = null)
    {
        Guard(reportKey, reportName, storedProc);
        return new Report
        {
            ReportKey = reportKey.Trim(),
            ReportName = reportName.Trim(),
            ReportGrouping = string.IsNullOrWhiteSpace(reportGrouping) ? "General" : reportGrouping.Trim(),
            StoredProc = storedProc.Trim(),
            SortOrder = sortOrder,
            Description = description,
            IsActive = isActive,
            GridConfig = string.IsNullOrWhiteSpace(gridConfig) ? null : gridConfig.Trim()
        };
    }

    public void Update(string reportKey, string reportName, string reportGrouping,
        string storedProc, int sortOrder, string? description, bool isActive, string? gridConfig = null)
    {
        Guard(reportKey, reportName, storedProc);
        ReportKey = reportKey.Trim();
        ReportName = reportName.Trim();
        ReportGrouping = string.IsNullOrWhiteSpace(reportGrouping) ? "General" : reportGrouping.Trim();
        StoredProc = storedProc.Trim();
        SortOrder = sortOrder;
        Description = description;
        IsActive = isActive;
        GridConfig = string.IsNullOrWhiteSpace(gridConfig) ? null : gridConfig.Trim();
        base.Update();
    }

    /// <summary>Replaces the parameter metadata (mirrors the reference ReportField child rows).</summary>
    public void SetFields(IEnumerable<ReportFieldSpec> fields)
    {
        _fields.Clear();
        var order = 1;
        foreach (var f in fields)
            _fields.Add(ReportField.Create(Id, f.Field, f.Label, f.DataType, f.FieldOrder ?? order++, f.DependencyField));
    }

    /// <summary>Replaces the selectable output-column metadata (reference ReportFieldOutput rows).</summary>
    public void SetFieldOutputs(IEnumerable<ReportFieldOutputSpec> outputs)
    {
        _fieldOutputs.Clear();
        var order = 1;
        foreach (var o in outputs)
            _fieldOutputs.Add(ReportFieldOutput.Create(Id, o.Field, o.Label, o.FieldOrder ?? order++));
    }

    private static void Guard(string reportKey, string reportName, string storedProc)
    {
        if (string.IsNullOrWhiteSpace(reportKey))
            throw new ArgumentException("Report key cannot be empty.", nameof(reportKey));
        if (string.IsNullOrWhiteSpace(reportName))
            throw new ArgumentException("Report name cannot be empty.", nameof(reportName));
        if (string.IsNullOrWhiteSpace(storedProc))
            throw new ArgumentException("Stored procedure cannot be empty.", nameof(storedProc));
    }
}

/// <summary>Input spec for one report parameter.</summary>
public record ReportFieldSpec(string Field, string Label, ReportFieldDataType DataType,
    int? FieldOrder = null, string? DependencyField = null);

/// <summary>Input spec for one selectable output column.</summary>
public record ReportFieldOutputSpec(string Field, string Label, int? FieldOrder = null);

/// <summary>
/// One selectable OUTPUT column of a report (reference ReportFieldOutput): the viewer's column
/// chooser; the user's selection is passed to the SP as the @OutputFields JSON array (the port of
/// the reference's pReportFieldOutput), letting the SP shape/limit its own output.
/// </summary>
public class ReportFieldOutput : BaseEntity
{
    public Guid ReportId { get; private set; }
    public string Field { get; private set; } = string.Empty;
    public string Label { get; private set; } = string.Empty;
    public int FieldOrder { get; private set; }

    private ReportFieldOutput() : base() { }

    public static ReportFieldOutput Create(Guid reportId, string field, string label, int fieldOrder)
    {
        if (string.IsNullOrWhiteSpace(field))
            throw new ArgumentException("Field cannot be empty.", nameof(field));
        return new ReportFieldOutput
        {
            ReportId = reportId,
            Field = field.Trim(),
            Label = string.IsNullOrWhiteSpace(label) ? field.Trim() : label.Trim(),
            FieldOrder = fieldOrder
        };
    }
}

/// <summary>
/// A SAVED FILTER SET (the reference's non-scheduled ReportClientSchedule "children"): a named
/// snapshot of criteria + chosen output columns that appears under its report in the catalog and
/// reloads the form prefilled.
/// </summary>
public class SavedReportFilter : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid ReportId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    /// <summary>JSON dict of field → value (the @Criteria payload).</summary>
    public string CriteriaJson { get; private set; } = "{}";
    /// <summary>JSON array of chosen output field names (null = all).</summary>
    public string? OutputFieldsJson { get; private set; }

    private SavedReportFilter() : base() { }

    public static SavedReportFilter Create(Guid reportId, string name, string criteriaJson, string? outputFieldsJson)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        return new SavedReportFilter
        {
            ReportId = reportId,
            Name = name.Trim(),
            CriteriaJson = string.IsNullOrWhiteSpace(criteriaJson) ? "{}" : criteriaJson,
            OutputFieldsJson = outputFieldsJson
        };
    }
}

/// <summary>Run-history row (reference ReportRun): who ran what, with which criteria, how fast.</summary>
public class ReportRun : BaseEntity, IAggregateRoot
{
    public string ReportKey { get; private set; } = string.Empty;
    public string CriteriaJson { get; private set; } = "{}";
    public int RowCount { get; private set; }
    public int DurationMs { get; private set; }
    public string? RanBy { get; private set; }
    /// <summary>reference ReportRun.IsScheduled — true when produced by a scheduled delivery, not an ad-hoc run.</summary>
    public bool IsScheduled { get; private set; }
    /// <summary>reference ReportRun.FieldOutput — packed selected output columns at run time.</summary>
    public string? FieldOutput { get; private set; }

    private ReportRun() : base() { }

    public static ReportRun Create(string reportKey, string criteriaJson, int rowCount, int durationMs, string? ranBy,
        bool isScheduled = false, string? fieldOutput = null) =>
        new()
        {
            ReportKey = reportKey,
            CriteriaJson = string.IsNullOrWhiteSpace(criteriaJson) ? "{}" : criteriaJson,
            RowCount = rowCount,
            DurationMs = durationMs,
            RanBy = ranBy,
            IsScheduled = isScheduled,
            FieldOutput = fieldOutput
        };
}

/// <summary>reference ReportRunRecipient: who a run's output was delivered to (scheduled/e-mailed).</summary>
public class ReportRunRecipient : BaseEntity
{
    public Guid ReportRunId { get; private set; }
    public Guid? UserId { get; private set; }
    public string Email { get; private set; } = string.Empty;

    private ReportRunRecipient() : base() { }

    public static ReportRunRecipient Create(Guid reportRunId, string email, Guid? userId = null) =>
        new() { ReportRunId = reportRunId, Email = email.Trim(), UserId = userId };
}

/// <summary>
/// One INPUT parameter of a <see cref="Report"/>. <see cref="Field"/> is the key inside the
/// @Criteria JSON; a '#' in the name declares a From/To RANGE pair (reference convention: the '#'
/// expands to '1' and '2', e.g. "HireDate#" → HireDate1/HireDate2). <see cref="DependencyField"/>
/// makes a Select cascade: its options reload when the named parent field changes.
/// </summary>
public class ReportField : BaseEntity
{
    public Guid ReportId { get; private set; }
    public string Field { get; private set; } = string.Empty;
    public string Label { get; private set; } = string.Empty;
    public ReportFieldDataType DataType { get; private set; } = ReportFieldDataType.Text;
    public int FieldOrder { get; private set; }
    public string? DependencyField { get; private set; }

    private ReportField() : base() { }

    public static ReportField Create(Guid reportId, string field, string label,
        ReportFieldDataType dataType, int fieldOrder, string? dependencyField = null)
    {
        if (string.IsNullOrWhiteSpace(field))
            throw new ArgumentException("Field cannot be empty.", nameof(field));
        return new ReportField
        {
            ReportId = reportId,
            Field = field.Trim(),
            Label = string.IsNullOrWhiteSpace(label) ? field.Trim() : label.Trim(),
            DataType = dataType,
            FieldOrder = fieldOrder,
            DependencyField = string.IsNullOrWhiteSpace(dependencyField) ? null : dependencyField.Trim()
        };
    }
}

/// <summary>
/// A SCHEDULED report delivery (reference ReportClientSchedule). Normalized exactly like the legacy:
/// the header carries the cadence (Frequency / FrequencyWeekly bitmask / TimeOfTheDay minutes-since-midnight
/// / ScheduleStartDate / OutputFormat) and mail body, while recipients, criteria field values, and selected
/// output columns live in the three child tables (<see cref="ReportScheduleRecipient"/>,
/// <see cref="ReportScheduleFieldValue"/>, <see cref="ReportScheduleFieldOutput"/>). The 5-part
/// <see cref="CronExpression"/> is derived from the cadence for Hangfire. CRUD is driven by the ported
/// legacy stored procedures (_x_ReportClientSchedule*), not EF change-tracking.
/// </summary>
public class ReportSchedule : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid ReportId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    /// <summary>reference IsScheduled — master on/off from the schedule form's "Enable schedule".</summary>
    public bool IsScheduled { get; private set; } = true;
    /// <summary>reference IsActive — the enable/disable toggle in the schedule grid.</summary>
    public bool IsActive { get; private set; } = true;
    public string? MailSubject { get; private set; }
    public string? MailBody { get; private set; }
    /// <summary>reference IsHideRecipients — BCC-style delivery, one mail per recipient.</summary>
    public bool IsHideRecipients { get; private set; }
    /// <summary>reference Frequency — Daily / Weekly / Monthly / Quarterly / Yearly.</summary>
    public string Frequency { get; private set; } = "Daily";
    /// <summary>reference FrequencyWeekly — day bitmask (Sun=64,Mon=32,Tue=16,Wed=8,Thu=4,Fri=2,Sat=1).</summary>
    public int FrequencyWeekly { get; private set; }
    /// <summary>reference TimeOfTheDay — minutes since midnight (hour24*60; minutes always :00).</summary>
    public int TimeOfTheDay { get; private set; }
    public DateOnly? ScheduleStartDate { get; private set; }
    /// <summary>reference OutputFormat — 1=CSV, 2=Excel, 3=PDF.</summary>
    public int OutputFormat { get; private set; } = 1;
    /// <summary>Derived 5-part cron (Hangfire) from the cadence above.</summary>
    public string CronExpression { get; private set; } = string.Empty;

    private ReportSchedule() : base() { }

    public void Deactivate() { IsActive = false; base.Update(); }
}

/// <summary>reference ReportClientScheduleRecipient: a schedule target — a user OR a role (never both).
/// <see cref="Email"/> is a snapshot resolved at save time so background delivery needs no user join.</summary>
public class ReportScheduleRecipient : BaseEntity
{
    public Guid ReportScheduleId { get; private set; }
    public Guid? UserId { get; private set; }
    public Guid? RoleId { get; private set; }
    public string? Email { get; private set; }

    private ReportScheduleRecipient() : base() { }

    public static ReportScheduleRecipient Create(Guid reportScheduleId, Guid? userId, Guid? roleId, string? email) =>
        new() { ReportScheduleId = reportScheduleId, UserId = userId, RoleId = roleId, Email = email?.Trim() };
}

/// <summary>reference ReportClientScheduleFieldValue: one saved criteria value (Field→Value) for the schedule.</summary>
public class ReportScheduleFieldValue : BaseEntity
{
    public Guid ReportScheduleId { get; private set; }
    public string ReportKey { get; private set; } = string.Empty;
    public string Field { get; private set; } = string.Empty;
    public string? Value { get; private set; }

    private ReportScheduleFieldValue() : base() { }

    public static ReportScheduleFieldValue Create(Guid reportScheduleId, string reportKey, string field, string? value) =>
        new() { ReportScheduleId = reportScheduleId, ReportKey = reportKey, Field = field.Trim(), Value = value };
}

/// <summary>reference ReportClientScheduleFieldOutput: one selected output column (with label/order/sort)
/// for the schedule's generated file.</summary>
public class ReportScheduleFieldOutput : BaseEntity
{
    public Guid ReportScheduleId { get; private set; }
    public string ReportKey { get; private set; } = string.Empty;
    public string Field { get; private set; } = string.Empty;
    public string Label { get; private set; } = string.Empty;
    public int FieldOrder { get; private set; }
    public int SortOrder { get; private set; }

    private ReportScheduleFieldOutput() : base() { }

    public static ReportScheduleFieldOutput Create(Guid reportScheduleId, string reportKey, string field,
        string label, int fieldOrder, int sortOrder) =>
        new()
        {
            ReportScheduleId = reportScheduleId,
            ReportKey = reportKey,
            Field = field.Trim(),
            Label = string.IsNullOrWhiteSpace(label) ? field.Trim() : label.Trim(),
            FieldOrder = fieldOrder,
            SortOrder = sortOrder
        };
}

/// <summary>
/// Restricts a report TO holders of a role (reference ReportClientRestriction): a report with any
/// restriction rows is visible/runnable ONLY by users holding one of the listed roles; a report
/// with none is open to everyone.
/// </summary>
public class ReportRestriction : BaseEntity
{
    public Guid ReportId { get; private set; }
    public Guid RoleId { get; private set; }
    /// <summary>Display snapshot so admin lists render without joins.</summary>
    public string RoleName { get; private set; } = string.Empty;

    private ReportRestriction() : base() { }

    public static ReportRestriction Create(Guid reportId, Guid roleId, string roleName) =>
        new() { ReportId = reportId, RoleId = roleId, RoleName = roleName };
}
