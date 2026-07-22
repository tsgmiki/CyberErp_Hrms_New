using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>
/// A structured learning path (HC193): an ordered sequence of catalog courses forming a long-term
/// development program, optionally aligned to a target position for career progression.
/// </summary>
public class LearningPath : BaseEntity, IAggregateRoot, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    /// <summary>Career alignment — the position this path prepares an employee for.</summary>
    public Guid? TargetPositionId { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly List<LearningPathStep> _steps = [];
    public IReadOnlyCollection<LearningPathStep> Steps => _steps;

    private LearningPath() : base() { }

    public static LearningPath Create(string name, string? description, Guid? targetPositionId, bool isActive = true)
    {
        Guard(name);
        return new LearningPath
        {
            Name = name,
            Description = description,
            TargetPositionId = targetPositionId,
            IsActive = isActive
        };
    }

    public void Update(string name, string? description, Guid? targetPositionId, bool isActive)
    {
        Guard(name);
        Name = name;
        Description = description;
        TargetPositionId = targetPositionId;
        IsActive = isActive;
        base.Update();
    }

    /// <summary>Replaces the ordered course sequence.</summary>
    public void SetSteps(IEnumerable<LearningPathStepSpec> specs)
    {
        var list = specs.ToList();
        if (list.Count == 0)
            throw new ArgumentException("A learning path needs at least one course.", nameof(specs));
        if (list.Select(s => s.TrainingCourseId).Distinct().Count() != list.Count)
            throw new ArgumentException("A course appears more than once on the path.", nameof(specs));

        _steps.Clear();
        var order = 1;
        foreach (var spec in list)
            _steps.Add(LearningPathStep.Create(Id, spec.TrainingCourseId, order++, spec.IsRequired));
        base.Update();
    }

    private static void Guard(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Path name cannot be empty.", nameof(name));
    }
}

public record LearningPathStepSpec(Guid TrainingCourseId, bool IsRequired);

/// <summary>One ordered course on a learning path.</summary>
public class LearningPathStep : BaseEntity
{
    public Guid LearningPathId { get; private set; }
    public Guid TrainingCourseId { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsRequired { get; private set; } = true;

    private LearningPathStep() : base() { }

    internal static LearningPathStep Create(Guid learningPathId, Guid trainingCourseId, int sortOrder, bool isRequired)
    {
        if (trainingCourseId == Guid.Empty)
            throw new ArgumentException("A course is required.", nameof(trainingCourseId));
        return new LearningPathStep
        {
            LearningPathId = learningPathId,
            TrainingCourseId = trainingCourseId,
            SortOrder = sortOrder,
            IsRequired = isRequired
        };
    }
}

/// <summary>
/// A certification held by an employee (HC200): issued from a completed enrollment (with its digital
/// certificate rendered by the document merge engine) or recorded manually for external credentials.
/// Expiry drives the renewal-tracking list.
/// </summary>
public class EmployeeTrainingCertificate : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid EmployeeId { get; private set; }
    public Guid? TrainingCourseId { get; private set; }
    public Guid? TrainingEnrollmentId { get; private set; }
    public string CertificateNo { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public DateTime IssuedOn { get; private set; }
    public DateTime? ExpiresOn { get; private set; }
    public string? Notes { get; private set; }

    private EmployeeTrainingCertificate() : base() { }

    public static EmployeeTrainingCertificate Create(Guid employeeId, string certificateNo, string title,
        DateTime issuedOn, DateTime? expiresOn, Guid? trainingCourseId = null, Guid? trainingEnrollmentId = null,
        string? notes = null)
    {
        Guard(employeeId, certificateNo, title, issuedOn, expiresOn);
        return new EmployeeTrainingCertificate
        {
            EmployeeId = employeeId,
            CertificateNo = certificateNo,
            Title = title,
            IssuedOn = issuedOn,
            ExpiresOn = expiresOn,
            TrainingCourseId = trainingCourseId,
            TrainingEnrollmentId = trainingEnrollmentId,
            Notes = notes
        };
    }

    public void Update(string title, DateTime issuedOn, DateTime? expiresOn, string? notes)
    {
        Guard(EmployeeId, CertificateNo, title, issuedOn, expiresOn);
        Title = title;
        IssuedOn = issuedOn;
        ExpiresOn = expiresOn;
        Notes = notes;
        base.Update();
    }

    /// <summary>HC200 — renewal pushes the expiry forward.</summary>
    public void Renew(DateTime newExpiresOn)
    {
        if (newExpiresOn <= (ExpiresOn ?? IssuedOn))
            throw new ArgumentException("Renewal must extend the current expiry.", nameof(newExpiresOn));
        ExpiresOn = newExpiresOn;
        base.Update();
    }

    private static void Guard(Guid employeeId, string certificateNo, string title, DateTime issuedOn, DateTime? expiresOn)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("An employee is required.", nameof(employeeId));
        if (string.IsNullOrWhiteSpace(certificateNo))
            throw new ArgumentException("A certificate number is required.", nameof(certificateNo));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("A certificate title is required.", nameof(title));
        if (expiresOn.HasValue && expiresOn.Value <= issuedOn)
            throw new ArgumentException("Expiry must fall after the issue date.", nameof(expiresOn));
    }
}

/// <summary>Payment state of a provider hand-off row (HC202).</summary>
public enum ProviderPaymentStatus
{
    Pending = 0,
    Paid = 1,
    Cancelled = 2
}

/// <summary>
/// The finance hand-off for an external training provider (HC202): one row per completed
/// provider-delivered session (or entered manually). There is no finance module — finance marks
/// rows paid or exports the batch.
/// </summary>
public class TrainingProviderPayment : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid? TrainingSessionId { get; private set; }
    public string ProviderName { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public ProviderPaymentStatus Status { get; private set; } = ProviderPaymentStatus.Pending;
    public DateTime? PaidAt { get; private set; }
    /// <summary>Payment reference (cheque / transfer number) captured when marked paid.</summary>
    public string? Reference { get; private set; }
    public string? Notes { get; private set; }

    private TrainingProviderPayment() : base() { }

    public static TrainingProviderPayment Create(string providerName, decimal amount,
        Guid? trainingSessionId = null, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(providerName))
            throw new ArgumentException("A provider is required.", nameof(providerName));
        if (amount <= 0)
            throw new ArgumentException("Payment amount must be positive.", nameof(amount));
        return new TrainingProviderPayment
        {
            ProviderName = providerName,
            Amount = amount,
            TrainingSessionId = trainingSessionId,
            Notes = notes
        };
    }

    public void MarkPaid(DateTime paidAt, string? reference)
    {
        if (Status != ProviderPaymentStatus.Pending)
            throw new InvalidOperationException($"Only a pending payment can be paid (current: {Status}).");
        Status = ProviderPaymentStatus.Paid;
        PaidAt = paidAt;
        Reference = reference;
        base.Update();
    }

    public void Cancel()
    {
        if (Status != ProviderPaymentStatus.Pending)
            throw new InvalidOperationException($"Only a pending payment can be cancelled (current: {Status}).");
        Status = ProviderPaymentStatus.Cancelled;
        base.Update();
    }
}
