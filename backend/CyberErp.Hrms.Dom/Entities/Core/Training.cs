using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>How a course is delivered (HC196).</summary>
public enum TrainingDeliveryMode
{
    InPerson = 0,
    Online = 1,
    Hybrid = 2
}

/// <summary>Where the training is delivered (HC187) — drives the approval chain (HC188/HC201).</summary>
public enum TrainingNeedType
{
    Local = 0,
    Abroad = 1
}

/// <summary>Lifecycle of a training need, request → fulfillment (HC188).</summary>
public enum TrainingNeedStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Fulfilled = 3,
    Cancelled = 4
}

public enum TrainingNeedPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

/// <summary>What surfaced the need (HC189) — manual request or a performance-driven suggestion.</summary>
public enum TrainingNeedSource
{
    Manual = 0,
    CompetencyGap = 1,
    Appraisal = 2,
    Goal = 3
}

/// <summary>
/// Groups catalog courses by kind (HC191), e.g. "Technical", "Leadership", "Compliance",
/// "Education Program".
/// </summary>
public class TrainingCategory : BaseEntity, IAggregateRoot, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int SortOrder { get; private set; }

    private TrainingCategory() : base() { }

    public static TrainingCategory Create(string name, string? description, bool isActive = true, int sortOrder = 0)
    {
        Guard(name);
        return new TrainingCategory
        {
            Name = name,
            Description = description,
            IsActive = isActive,
            SortOrder = sortOrder
        };
    }

    public void Update(string name, string? description, bool isActive, int sortOrder)
    {
        Guard(name);
        Name = name;
        Description = description;
        IsActive = isActive;
        SortOrder = sortOrder;
        base.Update();
    }

    private static void Guard(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be empty.", nameof(name));
    }
}

/// <summary>
/// A catalog course / program (HC191, HC196): objectives, audience, prerequisites, duration and
/// delivery mode. External courses reference their provider by name + URL (HC194 — metadata only,
/// no vendor API). CPD hours feed the professional-development rollup (HC200).
/// </summary>
public class TrainingCourse : BaseEntity, IAggregateRoot, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    public string? Code { get; private set; }
    public Guid? TrainingCategoryId { get; private set; }
    public string? Description { get; private set; }
    public string? Objectives { get; private set; }
    public string? TargetAudience { get; private set; }
    public string? Prerequisites { get; private set; }
    public decimal? DurationHours { get; private set; }
    public TrainingDeliveryMode DeliveryMode { get; private set; } = TrainingDeliveryMode.InPerson;
    /// <summary>CPD hours credited when a participant completes the course (HC200).</summary>
    public decimal CpdHours { get; private set; }
    public bool IsExternal { get; private set; }
    public string? ProviderName { get; private set; }
    public string? ExternalUrl { get; private set; }
    public bool IsActive { get; private set; } = true;

    private TrainingCourse() : base() { }

    public static TrainingCourse Create(string name, string? code, Guid? trainingCategoryId, string? description,
        string? objectives, string? targetAudience, string? prerequisites, decimal? durationHours,
        TrainingDeliveryMode deliveryMode, decimal cpdHours = 0, bool isExternal = false,
        string? providerName = null, string? externalUrl = null, bool isActive = true)
    {
        Guard(name, durationHours, cpdHours, isExternal, providerName);
        return new TrainingCourse
        {
            Name = name,
            Code = code,
            TrainingCategoryId = trainingCategoryId,
            Description = description,
            Objectives = objectives,
            TargetAudience = targetAudience,
            Prerequisites = prerequisites,
            DurationHours = durationHours,
            DeliveryMode = deliveryMode,
            CpdHours = cpdHours,
            IsExternal = isExternal,
            ProviderName = providerName,
            ExternalUrl = externalUrl,
            IsActive = isActive
        };
    }

    public void Update(string name, string? code, Guid? trainingCategoryId, string? description,
        string? objectives, string? targetAudience, string? prerequisites, decimal? durationHours,
        TrainingDeliveryMode deliveryMode, decimal cpdHours, bool isExternal,
        string? providerName, string? externalUrl, bool isActive)
    {
        Guard(name, durationHours, cpdHours, isExternal, providerName);
        Name = name;
        Code = code;
        TrainingCategoryId = trainingCategoryId;
        Description = description;
        Objectives = objectives;
        TargetAudience = targetAudience;
        Prerequisites = prerequisites;
        DurationHours = durationHours;
        DeliveryMode = deliveryMode;
        CpdHours = cpdHours;
        IsExternal = isExternal;
        ProviderName = providerName;
        ExternalUrl = externalUrl;
        IsActive = isActive;
        base.Update();
    }

    private static void Guard(string name, decimal? durationHours, decimal cpdHours, bool isExternal, string? providerName)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Course name cannot be empty.", nameof(name));
        if (durationHours is < 0)
            throw new ArgumentException("Duration cannot be negative.", nameof(durationHours));
        if (cpdHours < 0)
            throw new ArgumentException("CPD hours cannot be negative.", nameof(cpdHours));
        if (isExternal && string.IsNullOrWhiteSpace(providerName))
            throw new ArgumentException("An external course needs its provider named.", nameof(providerName));
    }
}

/// <summary>
/// A training / development need for one employee (HC187), routed through the per-type approval chain
/// ("TrainingNeed.Local" / "TrainingNeed.Abroad", HC188/HC201). Either references a catalog course or
/// carries a free-text topic. Tracked request → fulfillment; fulfillment comes from a completed
/// enrollment (Phase TD2).
/// </summary>
public class TrainingNeed : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid EmployeeId { get; private set; }
    public Guid? TrainingCourseId { get; private set; }
    /// <summary>Free-text topic when no catalog course fits.</summary>
    public string Topic { get; private set; } = string.Empty;
    public TrainingNeedType NeedType { get; private set; } = TrainingNeedType.Local;
    public string Justification { get; private set; } = string.Empty;
    public TrainingNeedPriority Priority { get; private set; } = TrainingNeedPriority.Medium;
    public TrainingNeedSource Source { get; private set; } = TrainingNeedSource.Manual;
    public TrainingNeedStatus Status { get; private set; } = TrainingNeedStatus.Pending;
    /// <summary>The competency gap that surfaced the need (HC189) — optional.</summary>
    public Guid? CompetencyId { get; private set; }
    /// <summary>Budget estimate counted against the training budget while unfulfilled (HC190).</summary>
    public decimal? EstimatedCost { get; private set; }
    public DateTime? NeededBy { get; private set; }
    /// <summary>Who raised the request — audit snapshot, intentionally no FK.</summary>
    public Guid? RequestedByEmployeeId { get; private set; }
    public DateTime? DecidedOn { get; private set; }
    public DateTime? FulfilledOn { get; private set; }

    private TrainingNeed() : base() { }

    public static TrainingNeed Create(Guid employeeId, string topic, TrainingNeedType needType, string justification,
        TrainingNeedPriority priority, TrainingNeedSource source, Guid? trainingCourseId = null,
        Guid? competencyId = null, decimal? estimatedCost = null, DateTime? neededBy = null,
        Guid? requestedByEmployeeId = null)
    {
        Guard(employeeId, topic, justification, estimatedCost);
        return new TrainingNeed
        {
            EmployeeId = employeeId,
            Topic = topic,
            NeedType = needType,
            Justification = justification,
            Priority = priority,
            Source = source,
            TrainingCourseId = trainingCourseId,
            CompetencyId = competencyId,
            EstimatedCost = estimatedCost,
            NeededBy = neededBy,
            RequestedByEmployeeId = requestedByEmployeeId
        };
    }

    /// <summary>Editable only while awaiting a decision.</summary>
    public void UpdateRequest(string topic, TrainingNeedType needType, string justification,
        TrainingNeedPriority priority, Guid? trainingCourseId, Guid? competencyId,
        decimal? estimatedCost, DateTime? neededBy)
    {
        Guard(EmployeeId, topic, justification, estimatedCost);
        EnsurePending();
        Topic = topic;
        NeedType = needType;
        Justification = justification;
        Priority = priority;
        TrainingCourseId = trainingCourseId;
        CompetencyId = competencyId;
        EstimatedCost = estimatedCost;
        NeededBy = neededBy;
        base.Update();
    }

    public void MarkApproved(DateTime decidedOn)
    {
        EnsurePending();
        Status = TrainingNeedStatus.Approved;
        DecidedOn = decidedOn;
        base.Update();
    }

    public void MarkRejected(DateTime decidedOn)
    {
        EnsurePending();
        Status = TrainingNeedStatus.Rejected;
        DecidedOn = decidedOn;
        base.Update();
    }

    /// <summary>Closes the loop (HC188) — set when a linked enrollment completes (Phase TD2).</summary>
    public void MarkFulfilled(DateTime fulfilledOn)
    {
        if (Status != TrainingNeedStatus.Approved)
            throw new InvalidOperationException($"Only an approved need can be fulfilled (current: {Status}).");
        Status = TrainingNeedStatus.Fulfilled;
        FulfilledOn = fulfilledOn;
        base.Update();
    }

    public void Cancel()
    {
        if (Status is not (TrainingNeedStatus.Pending or TrainingNeedStatus.Approved))
            throw new InvalidOperationException($"Only a pending or approved need can be cancelled (current: {Status}).");
        Status = TrainingNeedStatus.Cancelled;
        base.Update();
    }

    /// <summary>The per-type workflow key (HC188/HC201), e.g. "TrainingNeed.Abroad".</summary>
    public string WorkflowEntityType => $"TrainingNeed.{NeedType}";

    private void EnsurePending()
    {
        if (Status != TrainingNeedStatus.Pending)
            throw new InvalidOperationException($"Only a pending need can change (current: {Status}).");
    }

    private static void Guard(Guid employeeId, string topic, string justification, decimal? estimatedCost)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("An employee is required.", nameof(employeeId));
        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException("A training topic is required.", nameof(topic));
        if (string.IsNullOrWhiteSpace(justification))
            throw new ArgumentException("A justification is required.", nameof(justification));
        if (estimatedCost is < 0)
            throw new ArgumentException("Estimated cost cannot be negative.", nameof(estimatedCost));
    }
}
