using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Lifecycle of a calibration session: assembled → finalized (adjustments applied).</summary>
public enum CalibrationStatus
{
    Draft = 0,
    Finalized = 1
}

/// <summary>
/// A calibration &amp; moderation session (HC128–HC129): HR/managers review and normalize the appraisal
/// scores of a cohort (a review cycle, optionally scoped to an organization unit) before finalization.
/// Each <see cref="CalibrationItem"/> snapshots an appraisal's original score and can carry a calibrated
/// score plus a documented justification; finalizing applies the adjustments back to the appraisals.
/// </summary>
public class CalibrationSession : BaseEntity, IAggregateRoot, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    public Guid ReviewCycleId { get; private set; }
    public Guid? OrganizationUnitId { get; private set; }
    public CalibrationStatus Status { get; private set; } = CalibrationStatus.Draft;
    public string? Notes { get; private set; }
    public DateTime? FinalizedAt { get; private set; }

    private readonly List<CalibrationItem> _items = [];
    public IReadOnlyCollection<CalibrationItem> Items => _items;

    private CalibrationSession() : base() { }

    public static CalibrationSession Create(string name, Guid reviewCycleId, Guid? organizationUnitId, string? notes)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Calibration session name cannot be empty.", nameof(name));
        if (reviewCycleId == Guid.Empty)
            throw new ArgumentException("A review cycle is required.", nameof(reviewCycleId));
        return new CalibrationSession
        {
            Name = name,
            ReviewCycleId = reviewCycleId,
            OrganizationUnitId = organizationUnitId,
            Notes = notes
        };
    }

    public void UpdateMeta(string name, string? notes)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Calibration session name cannot be empty.", nameof(name));
        EnsureDraft();
        Name = name;
        Notes = notes;
        base.Update();
    }

    public void AddItem(Guid appraisalId, Guid employeeId, decimal? originalScore) =>
        _items.Add(CalibrationItem.Create(Id, appraisalId, employeeId, originalScore));

    public void Finalize()
    {
        EnsureDraft();
        Status = CalibrationStatus.Finalized;
        FinalizedAt = DateTime.UtcNow;
        base.Update();
    }

    public void EnsureDraft()
    {
        if (Status != CalibrationStatus.Draft)
            throw new ArgumentException("A finalized calibration session can no longer be modified.");
    }
}

/// <summary>One appraisal under calibration — original vs. calibrated score with a documented reason (HC129).</summary>
public class CalibrationItem : BaseEntity
{
    public Guid CalibrationSessionId { get; private set; }
    public Guid AppraisalId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public decimal? OriginalScore { get; private set; }
    public decimal? CalibratedScore { get; private set; }
    public string? Justification { get; private set; }
    public bool IsAdjusted { get; private set; }

    private CalibrationItem() : base() { }

    public static CalibrationItem Create(Guid calibrationSessionId, Guid appraisalId, Guid employeeId, decimal? originalScore)
    {
        if (appraisalId == Guid.Empty)
            throw new ArgumentException("An appraisal is required.", nameof(appraisalId));
        return new CalibrationItem
        {
            CalibrationSessionId = calibrationSessionId,
            AppraisalId = appraisalId,
            EmployeeId = employeeId,
            OriginalScore = originalScore
        };
    }

    public void Calibrate(decimal? calibratedScore, string? justification)
    {
        CalibratedScore = calibratedScore;
        Justification = justification;
        IsAdjusted = calibratedScore.HasValue;
        base.Update();
    }
}
