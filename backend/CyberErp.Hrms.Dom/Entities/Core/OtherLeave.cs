using System.Text.Json.Serialization;
using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Who may take a given other-leave type (gender-specific statutory leaves).</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GenderEligibility
{
    All = 0,
    /// <summary>Female employees only (e.g. maternity leave).</summary>
    Female = 1,
    /// <summary>Male employees only (e.g. paternity leave).</summary>
    Male = 2
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OtherLeaveStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Cancelled = 3
}

/// <summary>How a leave block is costed: skip holidays/weekends or count every calendar day.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LeaveDayCounting
{
    /// <summary>Holidays and rest days are SKIPPED — only working days are charged.</summary>
    WorkingDays = 0,
    /// <summary>Every calendar day is charged — holidays and weekends COUNT (e.g. 90 consecutive days).</summary>
    CalendarDays = 1
}

/// <summary>
/// Policy for one NON-annual leave type in one fiscal year (hrmsOtherLeaveSetting). Unlike annual
/// leave these entitlements are STATIC — they never accrue/increment: the employee simply holds
/// <see cref="StandardDays"/> (or <see cref="ManagerialDays"/> for managerial positions) for the
/// year. Usage never touches the annual-leave ledger — it only draws down this allocation.
/// </summary>
public class OtherLeaveSetting : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid FiscalYearId { get; private set; }
    /// <summary>The leave type this policy governs (the LeaveType master relationship).</summary>
    public Guid LeaveTypeId { get; private set; }
    public GenderEligibility Gender { get; private set; } = GenderEligibility.All;
    /// <summary>Static allocation for non-managerial positions.</summary>
    public decimal StandardDays { get; private set; }
    /// <summary>Static allocation for managerial positions (≥ standard by convention).</summary>
    public decimal ManagerialDays { get; private set; }
    /// <summary>Maternity/paternity/mourning style: the whole entitlement is taken in ONE block.</summary>
    public bool IsLumpSum { get; private set; }
    /// <summary>Whether holidays/weekends are skipped (WorkingDays) or charged (CalendarDays).</summary>
    public LeaveDayCounting DayCounting { get; private set; } = LeaveDayCounting.WorkingDays;
    public bool IsActive { get; private set; } = true;
    public string? Description { get; private set; }

    private FiscalYear? _fiscalYear;
    public FiscalYear? FiscalYear => _fiscalYear;

    private LeaveType? _leaveType;
    public LeaveType? LeaveType => _leaveType;

    private OtherLeaveSetting() : base() { }

    public static OtherLeaveSetting Create(Guid fiscalYearId, Guid leaveTypeId, GenderEligibility gender,
        decimal standardDays, decimal managerialDays, bool isLumpSum, LeaveDayCounting dayCounting,
        bool isActive, string? description)
    {
        Guard(fiscalYearId, leaveTypeId, standardDays, managerialDays);
        return new OtherLeaveSetting
        {
            FiscalYearId = fiscalYearId,
            LeaveTypeId = leaveTypeId,
            Gender = gender,
            StandardDays = standardDays,
            ManagerialDays = managerialDays,
            IsLumpSum = isLumpSum,
            DayCounting = dayCounting,
            IsActive = isActive,
            Description = description
        };
    }

    public void Update(Guid fiscalYearId, Guid leaveTypeId, GenderEligibility gender,
        decimal standardDays, decimal managerialDays, bool isLumpSum, LeaveDayCounting dayCounting,
        bool isActive, string? description)
    {
        Guard(fiscalYearId, leaveTypeId, standardDays, managerialDays);
        FiscalYearId = fiscalYearId;
        LeaveTypeId = leaveTypeId;
        Gender = gender;
        StandardDays = standardDays;
        ManagerialDays = managerialDays;
        IsLumpSum = isLumpSum;
        DayCounting = dayCounting;
        IsActive = isActive;
        Description = description;
        base.Update();
    }

    /// <summary>The static allocation the given employee kind holds — no accrual, ever.</summary>
    public decimal AllocationFor(bool isManagerial) => isManagerial ? ManagerialDays : StandardDays;

    private static void Guard(Guid fiscalYearId, Guid leaveTypeId, decimal standardDays, decimal managerialDays)
    {
        if (fiscalYearId == Guid.Empty)
            throw new ArgumentException("A fiscal year is required.", nameof(fiscalYearId));
        if (leaveTypeId == Guid.Empty)
            throw new ArgumentException("The leave type is required.", nameof(leaveTypeId));
        if (standardDays <= 0)
            throw new ArgumentException("Standard days must be positive.", nameof(standardDays));
        if (managerialDays <= 0)
            throw new ArgumentException("Managerial days must be positive.", nameof(managerialDays));
    }
}

/// <summary>
/// HEADER of an Other-Leave request (hrmsOtherLeave, Master-Detail — mirrors AnnualLeaveHeader).
/// The referenced <see cref="OtherLeaveSetting"/> fixes fiscal year, gender rule and the static
/// allocation, so the request carries no leave-type or ledger fields. Approval draws down the
/// setting's allocation only — the annual-leave ledger is NEVER touched.
/// </summary>
public class OtherLeaveHeader : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid EmployeeId { get; private set; }
    public Guid OtherLeaveSettingId { get; private set; }
    public DateTime RequestDate { get; private set; }
    public string? Remark { get; private set; }
    /// <summary>Denormalized Σ of the detail rows' working days, snapshotted at submission.</summary>
    public decimal TotalLeaveDays { get; private set; }
    public OtherLeaveStatus Status { get; private set; } = OtherLeaveStatus.Pending;

    private readonly List<OtherLeaveDetail> _details = [];
    public IReadOnlyCollection<OtherLeaveDetail> Details => _details;

    /// <summary>
    /// Supporting documents (medical certificate, death certificate…). Read-only here: rows are
    /// inserted through their own repository at submission, the way medical-claim attachments are,
    /// so a child never depends on the aggregate to carry its TenantId. This navigation exists so
    /// the header projection can read the metadata in one query.
    /// </summary>
    private readonly List<OtherLeaveAttachment> _attachments = [];
    public IReadOnlyCollection<OtherLeaveAttachment> Attachments => _attachments;

    private Employee? _employee;
    public Employee? Employee => _employee;

    private OtherLeaveSetting? _setting;
    public OtherLeaveSetting? Setting => _setting;

    private OtherLeaveHeader() : base() { }

    public static OtherLeaveHeader Create(Guid employeeId, Guid otherLeaveSettingId, DateTime requestDate, string? remark)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee is required.", nameof(employeeId));
        if (otherLeaveSettingId == Guid.Empty)
            throw new ArgumentException("The leave setting is required.", nameof(otherLeaveSettingId));
        return new OtherLeaveHeader
        {
            EmployeeId = employeeId,
            OtherLeaveSettingId = otherLeaveSettingId,
            RequestDate = requestDate.Date,
            Remark = remark,
            Status = OtherLeaveStatus.Pending
        };
    }

    /// <summary>Adds a detail row and re-totals the request.</summary>
    public void AddDetail(DateTime startDate, DateTime endDate, decimal leaveDays)
    {
        _details.Add(OtherLeaveDetail.Create(Id, startDate, endDate, leaveDays));
        TotalLeaveDays = _details.Sum(d => d.LeaveDays);
    }

    public void Approve()
    {
        RequireStatus(OtherLeaveStatus.Pending);
        Status = OtherLeaveStatus.Approved;
        base.Update();
    }

    public void Reject()
    {
        RequireStatus(OtherLeaveStatus.Pending);
        Status = OtherLeaveStatus.Rejected;
        base.Update();
    }

    public void Cancel()
    {
        if (Status is not (OtherLeaveStatus.Pending or OtherLeaveStatus.Approved))
            throw new InvalidOperationException($"Only a pending or approved request can be cancelled (current: {Status}).");
        Status = OtherLeaveStatus.Cancelled;
        base.Update();
    }

    private void RequireStatus(OtherLeaveStatus expected)
    {
        if (Status != expected)
            throw new InvalidOperationException($"Expected status {expected} but was {Status}.");
    }
}

/// <summary>DETAIL row of an <see cref="OtherLeaveHeader"/> — one full-day date range (hrmsOtherLeaveDetail).</summary>
public class OtherLeaveDetail : BaseEntity
{
    public Guid OtherLeaveHeaderId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    /// <summary>Chargeable working days for this row (weekends/holidays excluded).</summary>
    public decimal LeaveDays { get; private set; }

    private OtherLeaveDetail() : base() { }

    public static OtherLeaveDetail Create(Guid otherLeaveHeaderId, DateTime startDate, DateTime endDate, decimal leaveDays)
    {
        if (endDate.Date < startDate.Date)
            throw new ArgumentException("End date cannot be before start date.", nameof(endDate));
        if (leaveDays <= 0)
            throw new ArgumentException("A detail row must span at least one working day.", nameof(leaveDays));
        return new OtherLeaveDetail
        {
            OtherLeaveHeaderId = otherLeaveHeaderId,
            StartDate = startDate.Date,
            EndDate = endDate.Date,
            LeaveDays = leaveDays
        };
    }
}

/// <summary>
/// A supporting document uploaded with an <see cref="OtherLeaveHeader"/> — the medical certificate
/// behind sick leave, the death certificate behind mourning leave, and so on.
///
/// <para>Stored as bytes in the row, matching <c>MedicalClaimAttachment</c> and
/// <c>InsuranceClaimAttachment</c>. That keeps the document inside the same transaction and the same
/// tenant boundary as the request it proves: an approver's decision and its evidence can never drift
/// apart, and no separate file store has to be backed up or access-controlled in step with the DB.</para>
/// </summary>
public class OtherLeaveAttachment : BaseEntity
{
    public Guid OtherLeaveHeaderId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = "application/octet-stream";
    public long FileSize { get; private set; }
    public byte[] Content { get; private set; } = [];

    private OtherLeaveAttachment() : base() { }

    public static OtherLeaveAttachment Create(Guid otherLeaveHeaderId, string fileName, string? contentType, byte[] content)
    {
        if (otherLeaveHeaderId == Guid.Empty)
            throw new ArgumentException("The leave request is required.", nameof(otherLeaveHeaderId));
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name is required.", nameof(fileName));
        if (content is null || content.Length == 0)
            throw new ArgumentException("File content is required.", nameof(content));
        return new OtherLeaveAttachment
        {
            OtherLeaveHeaderId = otherLeaveHeaderId,
            FileName = fileName.Trim(),
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
            FileSize = content.Length,
            Content = content
        };
    }
}
