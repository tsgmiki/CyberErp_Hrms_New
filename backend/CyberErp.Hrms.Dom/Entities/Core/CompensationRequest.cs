using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>What an employee's self-service compensation request is about (HC234).</summary>
public enum CompensationRequestType
{
    /// <summary>A request to change benefit enrollment/election.</summary>
    BenefitChange = 0,
    /// <summary>A reported discrepancy in pay/deductions.</summary>
    PayrollDiscrepancy = 1
}

/// <summary>Lifecycle of a compensation request.</summary>
public enum CompensationRequestStatus
{
    Submitted = 0,
    UnderReview = 1,
    Resolved = 2,
    Rejected = 3
}

/// <summary>
/// HC234 — a self-service request an employee raises about their compensation: a benefit change or a
/// reported payroll discrepancy. HR reviews and resolves it. Ticket-style (not an approval chain):
/// the resolution is HR's action/response.
/// </summary>
public class CompensationRequest : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid EmployeeId { get; private set; }
    public CompensationRequestType RequestType { get; private set; }
    public string Subject { get; private set; } = string.Empty;
    public string Details { get; private set; } = string.Empty;
    /// <summary>For a benefit change — the plan concerned (optional).</summary>
    public Guid? BenefitPlanId { get; private set; }
    /// <summary>For a payroll discrepancy — the pay period label, e.g. "2026-06" (optional).</summary>
    public string? ReferencePeriod { get; private set; }
    public decimal? DisputedAmount { get; private set; }
    public CompensationRequestStatus Status { get; private set; } = CompensationRequestStatus.Submitted;
    public string? Resolution { get; private set; }
    public DateTime SubmittedOn { get; private set; }
    public DateTime? ResolvedOn { get; private set; }

    private CompensationRequest() : base() { }

    public static CompensationRequest Create(Guid employeeId, CompensationRequestType type, string subject,
        string details, Guid? benefitPlanId, string? referencePeriod, decimal? disputedAmount, DateTime submittedOn)
    {
        if (employeeId == Guid.Empty) throw new ArgumentException("Employee is required.", nameof(employeeId));
        if (string.IsNullOrWhiteSpace(subject)) throw new ArgumentException("Subject is required.", nameof(subject));
        if (string.IsNullOrWhiteSpace(details)) throw new ArgumentException("Details are required.", nameof(details));
        if (disputedAmount is < 0) throw new ArgumentException("Disputed amount cannot be negative.", nameof(disputedAmount));
        return new CompensationRequest
        {
            EmployeeId = employeeId,
            RequestType = type,
            Subject = subject.Trim(),
            Details = details.Trim(),
            BenefitPlanId = benefitPlanId,
            ReferencePeriod = referencePeriod,
            DisputedAmount = disputedAmount,
            Status = CompensationRequestStatus.Submitted,
            SubmittedOn = submittedOn
        };
    }

    public void StartReview()
    {
        if (Status is CompensationRequestStatus.Resolved or CompensationRequestStatus.Rejected)
            throw new InvalidOperationException("A closed request cannot be reopened for review.");
        Status = CompensationRequestStatus.UnderReview;
        base.Update();
    }

    public void Resolve(string resolution, DateTime resolvedOn)
    {
        if (string.IsNullOrWhiteSpace(resolution)) throw new ArgumentException("A resolution is required.", nameof(resolution));
        Status = CompensationRequestStatus.Resolved;
        Resolution = resolution.Trim();
        ResolvedOn = resolvedOn;
        base.Update();
    }

    public void Reject(string resolution, DateTime resolvedOn)
    {
        if (string.IsNullOrWhiteSpace(resolution)) throw new ArgumentException("A reason is required.", nameof(resolution));
        Status = CompensationRequestStatus.Rejected;
        Resolution = resolution.Trim();
        ResolvedOn = resolvedOn;
        base.Update();
    }
}
