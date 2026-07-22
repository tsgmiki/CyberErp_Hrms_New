using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Scope of a business trip (HC260) — drives per-diem rates and workflow.</summary>
public enum TripType
{
    Local = 0,
    International = 1
}

/// <summary>
/// HC267 — the daily per-diem rate for a job grade on a given trip type (local/international). Trip
/// per-diem is computed from the traveller's grade × trip type × number of days.
/// </summary>
public class PerDiemRate : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid JobGradeId { get; private set; }
    public TripType TripType { get; private set; }
    public decimal DailyRate { get; private set; }
    public string Currency { get; private set; } = "ETB";
    public bool IsActive { get; private set; } = true;

    private PerDiemRate() : base() { }

    public static PerDiemRate Create(Guid jobGradeId, TripType tripType, decimal dailyRate, string? currency, bool isActive = true)
    {
        Guard(jobGradeId, dailyRate);
        return new PerDiemRate
        {
            JobGradeId = jobGradeId,
            TripType = tripType,
            DailyRate = dailyRate,
            Currency = string.IsNullOrWhiteSpace(currency) ? "ETB" : currency.Trim(),
            IsActive = isActive
        };
    }

    public void Update(Guid jobGradeId, TripType tripType, decimal dailyRate, string? currency, bool isActive)
    {
        Guard(jobGradeId, dailyRate);
        JobGradeId = jobGradeId;
        TripType = tripType;
        DailyRate = dailyRate;
        Currency = string.IsNullOrWhiteSpace(currency) ? "ETB" : currency.Trim();
        IsActive = isActive;
        base.Update();
    }

    private static void Guard(Guid jobGradeId, decimal dailyRate)
    {
        if (jobGradeId == Guid.Empty) throw new ArgumentException("Job grade is required.", nameof(jobGradeId));
        if (dailyRate < 0) throw new ArgumentException("Daily rate cannot be negative.", nameof(dailyRate));
    }
}

/// <summary>
/// HC266 — a travel budget allocated to an organization unit (or the whole organization when the unit is
/// null) for a fiscal year. Utilization is computed from trip advances/expenses, not stored.
/// </summary>
public class TripBudget : BaseEntity, IAggregateRoot, IAuditable
{
    public int FiscalYear { get; private set; }
    /// <summary>Null = organization-wide budget.</summary>
    public Guid? OrganizationUnitId { get; private set; }
    public decimal Amount { get; private set; }
    public string? Notes { get; private set; }

    private TripBudget() : base() { }

    public static TripBudget Create(int fiscalYear, Guid? organizationUnitId, decimal amount, string? notes)
    {
        Guard(fiscalYear, amount);
        return new TripBudget
        {
            FiscalYear = fiscalYear,
            OrganizationUnitId = organizationUnitId,
            Amount = amount,
            Notes = notes
        };
    }

    public void Update(int fiscalYear, Guid? organizationUnitId, decimal amount, string? notes)
    {
        Guard(fiscalYear, amount);
        FiscalYear = fiscalYear;
        OrganizationUnitId = organizationUnitId;
        Amount = amount;
        Notes = notes;
        base.Update();
    }

    private static void Guard(int fiscalYear, decimal amount)
    {
        if (fiscalYear < 2000 || fiscalYear > 3000) throw new ArgumentException("Fiscal year is out of range.", nameof(fiscalYear));
        if (amount < 0) throw new ArgumentException("Budget amount cannot be negative.", nameof(amount));
    }
}

/// <summary>Lifecycle of a business trip request (HC260).</summary>
public enum TripRequestStatus
{
    Requested = 0,
    Approved = 1,
    Rejected = 2,
    InProgress = 3,
    Completed = 4,
    Settled = 5,
    Cancelled = 6
}

/// <summary>
/// HC260–HC262 — an employee's business trip: destination, dates, computed per-diem (grade × days),
/// requested advance and the actual expenses. Moves Requested → Approved/Rejected → InProgress →
/// Completed → Settled. Draws on a <see cref="TripBudget"/> (resolved at request time).
/// </summary>
public class TripRequest : BaseEntity, IAggregateRoot, IAuditable
{
    public string TripNumber { get; private set; } = string.Empty;
    public Guid EmployeeId { get; private set; }
    public TripType TripType { get; private set; }
    public string Destination { get; private set; } = string.Empty;
    public string? Purpose { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public int Days { get; private set; }
    public decimal DailyPerDiemRate { get; private set; }
    public decimal PerDiemAmount { get; private set; }
    public decimal AdvanceAmount { get; private set; }
    public string Currency { get; private set; } = "ETB";
    public Guid? TripBudgetId { get; private set; }
    public TripRequestStatus Status { get; private set; } = TripRequestStatus.Requested;
    public string? Resolution { get; private set; }
    public DateTime RequestDate { get; private set; }
    // T3 fields (advance disbursement / settlement) — present now so the schema is stable.
    public DateTime? AdvanceDisbursedAt { get; private set; }
    public string? AdvanceReference { get; private set; }
    public DateTime? SettledAt { get; private set; }
    public decimal? SettlementNet { get; private set; }
    public string? SettlementReference { get; private set; }

    private readonly List<TripExpense> _expenses = [];
    public IReadOnlyCollection<TripExpense> Expenses => _expenses;

    private TripRequest() : base() { }

    public static TripRequest Create(string tripNumber, Guid employeeId, TripType tripType, string destination,
        string? purpose, DateTime startDate, DateTime endDate, int days, decimal dailyRate, decimal perDiem,
        decimal advance, string? currency, Guid? tripBudgetId, DateTime requestDate)
    {
        if (employeeId == Guid.Empty) throw new ArgumentException("Employee is required.", nameof(employeeId));
        if (string.IsNullOrWhiteSpace(destination)) throw new ArgumentException("Destination is required.", nameof(destination));
        if (endDate < startDate) throw new ArgumentException("End date cannot precede the start date.", nameof(endDate));
        if (advance < 0) throw new ArgumentException("Advance cannot be negative.", nameof(advance));
        return new TripRequest
        {
            TripNumber = tripNumber,
            EmployeeId = employeeId,
            TripType = tripType,
            Destination = destination.Trim(),
            Purpose = purpose,
            StartDate = startDate,
            EndDate = endDate,
            Days = days,
            DailyPerDiemRate = dailyRate,
            PerDiemAmount = perDiem,
            AdvanceAmount = advance,
            Currency = string.IsNullOrWhiteSpace(currency) ? "ETB" : currency.Trim(),
            TripBudgetId = tripBudgetId,
            Status = TripRequestStatus.Requested,
            RequestDate = requestDate
        };
    }

    public void Approve(string? note)
    {
        if (Status != TripRequestStatus.Requested) throw new InvalidOperationException("Only a requested trip can be approved.");
        Status = TripRequestStatus.Approved;
        Resolution = note;
        base.Update();
    }

    public void Reject(string reason)
    {
        if (Status != TripRequestStatus.Requested) throw new InvalidOperationException("Only a requested trip can be rejected.");
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("A rejection reason is required.", nameof(reason));
        Status = TripRequestStatus.Rejected;
        Resolution = reason.Trim();
        base.Update();
    }

    public void Cancel()
    {
        if (Status is not (TripRequestStatus.Requested or TripRequestStatus.Approved))
            throw new InvalidOperationException("Only a requested or approved trip can be cancelled.");
        Status = TripRequestStatus.Cancelled;
        base.Update();
    }

    public void Start()
    {
        if (Status != TripRequestStatus.Approved) throw new InvalidOperationException("Only an approved trip can start.");
        Status = TripRequestStatus.InProgress;
        base.Update();
    }

    public void Complete()
    {
        if (Status != TripRequestStatus.InProgress) throw new InvalidOperationException("Only an in-progress trip can be completed.");
        Status = TripRequestStatus.Completed;
        base.Update();
    }

    /// <summary>HC268 — records the advance payment (finance/CBS hand-off).</summary>
    public void DisburseAdvance(DateTime date, string? reference)
    {
        if (Status is not (TripRequestStatus.Approved or TripRequestStatus.InProgress))
            throw new InvalidOperationException("The advance can only be paid on an approved or in-progress trip.");
        AdvanceDisbursedAt = date;
        AdvanceReference = reference;
        base.Update();
    }

    /// <summary>HC264/HC268 — settles the trip (advance reconciled against actual expenses).</summary>
    public void Settle(DateTime date, decimal net, string? reference)
    {
        if (Status is not (TripRequestStatus.Completed or TripRequestStatus.InProgress))
            throw new InvalidOperationException("Only a completed trip can be settled.");
        Status = TripRequestStatus.Settled;
        SettledAt = date;
        SettlementNet = net;
        SettlementReference = reference;
        base.Update();
    }
}

/// <summary>HC262 — one expense line incurred during a trip (transport, accommodation, meals…).</summary>
public class TripExpense : BaseEntity
{
    public Guid TripRequestId { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime ExpenseDate { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "ETB";

    private TripExpense() : base() { }

    public static TripExpense Create(Guid tripId, string category, string? description, DateTime expenseDate, decimal amount, string? currency)
    {
        if (tripId == Guid.Empty) throw new ArgumentException("Trip is required.", nameof(tripId));
        if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("Category is required.", nameof(category));
        if (amount < 0) throw new ArgumentException("Amount cannot be negative.", nameof(amount));
        return new TripExpense
        {
            TripRequestId = tripId,
            Category = category.Trim(),
            Description = description,
            ExpenseDate = expenseDate,
            Amount = amount,
            Currency = string.IsNullOrWhiteSpace(currency) ? "ETB" : currency.Trim()
        };
    }
}
