using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Lifecycle of an employee loan (HC251/HC258).</summary>
public enum LoanStatus
{
    Requested = 0,
    Approved = 1,
    Rejected = 2,
    Disbursed = 3,
    Active = 4,
    Settled = 5,
    Cancelled = 6
}

/// <summary>Payment state of one scheduled repayment installment.</summary>
public enum LoanInstallmentStatus
{
    Pending = 0,
    Paid = 1
}

/// <summary>
/// HC251 — a configurable staff-loan product with its limits (max amount, max salary multiple, term),
/// interest rate (flat; 0 = interest-free) and service-commitment period. Loan requests reference a type.
/// </summary>
public class LoanType : BaseEntity, IAggregateRoot, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    /// <summary>Absolute ceiling on the principal; null = no absolute cap.</summary>
    public decimal? MaxAmount { get; private set; }
    /// <summary>Cap as a multiple of monthly base salary (e.g. 3 = 3× salary); null = not applied.</summary>
    public decimal? MaxSalaryMultiple { get; private set; }
    public int MaxTermMonths { get; private set; }
    /// <summary>Flat annual interest rate percent; 0 = interest-free.</summary>
    public decimal InterestRatePct { get; private set; }
    public bool RequiresGuarantor { get; private set; }
    public int MinGuarantors { get; private set; }
    /// <summary>Months of service the borrower commits to after endorsement (HC257/HC258); 0 = none.</summary>
    public int ServiceCommitmentMonths { get; private set; }
    public bool IsActive { get; private set; } = true;

    private LoanType() : base() { }

    public static LoanType Create(string name, string? description, decimal? maxAmount, decimal? maxSalaryMultiple,
        int maxTermMonths, decimal interestRatePct, bool requiresGuarantor, int minGuarantors,
        int serviceCommitmentMonths, bool isActive = true)
    {
        Guard(name, maxAmount, maxSalaryMultiple, maxTermMonths, interestRatePct, minGuarantors, serviceCommitmentMonths);
        return new LoanType
        {
            Name = name.Trim(),
            Description = description,
            MaxAmount = maxAmount,
            MaxSalaryMultiple = maxSalaryMultiple,
            MaxTermMonths = maxTermMonths,
            InterestRatePct = interestRatePct,
            RequiresGuarantor = requiresGuarantor,
            MinGuarantors = minGuarantors,
            ServiceCommitmentMonths = serviceCommitmentMonths,
            IsActive = isActive
        };
    }

    public void Update(string name, string? description, decimal? maxAmount, decimal? maxSalaryMultiple,
        int maxTermMonths, decimal interestRatePct, bool requiresGuarantor, int minGuarantors,
        int serviceCommitmentMonths, bool isActive)
    {
        Guard(name, maxAmount, maxSalaryMultiple, maxTermMonths, interestRatePct, minGuarantors, serviceCommitmentMonths);
        Name = name.Trim();
        Description = description;
        MaxAmount = maxAmount;
        MaxSalaryMultiple = maxSalaryMultiple;
        MaxTermMonths = maxTermMonths;
        InterestRatePct = interestRatePct;
        RequiresGuarantor = requiresGuarantor;
        MinGuarantors = minGuarantors;
        ServiceCommitmentMonths = serviceCommitmentMonths;
        IsActive = isActive;
        base.Update();
    }

    private static void Guard(string name, decimal? maxAmount, decimal? maxSalaryMultiple, int maxTermMonths,
        decimal interestRatePct, int minGuarantors, int serviceCommitmentMonths)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Loan type name is required.", nameof(name));
        if (maxAmount is < 0) throw new ArgumentException("Max amount cannot be negative.", nameof(maxAmount));
        if (maxSalaryMultiple is < 0) throw new ArgumentException("Max salary multiple cannot be negative.", nameof(maxSalaryMultiple));
        if (maxTermMonths <= 0) throw new ArgumentException("Max term must be positive.", nameof(maxTermMonths));
        if (interestRatePct is < 0) throw new ArgumentException("Interest rate cannot be negative.", nameof(interestRatePct));
        if (minGuarantors < 0) throw new ArgumentException("Min guarantors cannot be negative.", nameof(minGuarantors));
        if (serviceCommitmentMonths < 0) throw new ArgumentException("Service commitment cannot be negative.", nameof(serviceCommitmentMonths));
    }
}

/// <summary>
/// HC251–HC253 — an employee's loan: principal, term, flat interest, computed monthly installment and its
/// amortization schedule + guarantors. Moves Requested → Approved/Rejected → Disbursed → Active → Settled.
/// </summary>
public class Loan : BaseEntity, IAggregateRoot, IAuditable
{
    public string LoanNumber { get; private set; } = string.Empty;
    public Guid EmployeeId { get; private set; }
    public Guid LoanTypeId { get; private set; }
    public decimal PrincipalAmount { get; private set; }
    public int TermMonths { get; private set; }
    public decimal InterestRatePct { get; private set; }
    public decimal MonthlyInstallment { get; private set; }
    public decimal TotalInterest { get; private set; }
    public decimal TotalRepayable { get; private set; }
    public string? Purpose { get; private set; }
    public DateTime RequestDate { get; private set; }
    public LoanStatus Status { get; private set; } = LoanStatus.Requested;
    public string? Resolution { get; private set; }
    public int ServiceCommitmentMonths { get; private set; }
    // L2 fields (disbursement / consent) — present now so the schema is stable.
    public DateTime? DisbursedAt { get; private set; }
    public string? DisbursementReference { get; private set; }
    public DateTime? SettledAt { get; private set; }
    public DateTime? ServiceCommitmentConsentAt { get; private set; }

    private readonly List<LoanGuarantor> _guarantors = [];
    public IReadOnlyCollection<LoanGuarantor> Guarantors => _guarantors;
    private readonly List<LoanRepaymentScheduleLine> _schedule = [];
    public IReadOnlyCollection<LoanRepaymentScheduleLine> Schedule => _schedule;

    private Loan() : base() { }

    public static Loan Create(string loanNumber, Guid employeeId, Guid loanTypeId, decimal principal,
        int termMonths, decimal interestRatePct, int serviceCommitmentMonths, string? purpose, DateTime requestDate)
    {
        if (employeeId == Guid.Empty) throw new ArgumentException("Employee is required.", nameof(employeeId));
        if (loanTypeId == Guid.Empty) throw new ArgumentException("Loan type is required.", nameof(loanTypeId));
        if (principal <= 0) throw new ArgumentException("Principal must be positive.", nameof(principal));
        if (termMonths <= 0) throw new ArgumentException("Term must be positive.", nameof(termMonths));
        if (interestRatePct < 0) throw new ArgumentException("Interest rate cannot be negative.", nameof(interestRatePct));

        var totalInterest = Math.Round(principal * interestRatePct / 100m * termMonths / 12m, 2);
        var totalRepayable = principal + totalInterest;
        var monthly = Math.Round(totalRepayable / termMonths, 2);
        return new Loan
        {
            LoanNumber = loanNumber,
            EmployeeId = employeeId,
            LoanTypeId = loanTypeId,
            PrincipalAmount = principal,
            TermMonths = termMonths,
            InterestRatePct = interestRatePct,
            TotalInterest = totalInterest,
            TotalRepayable = totalRepayable,
            MonthlyInstallment = monthly,
            ServiceCommitmentMonths = serviceCommitmentMonths,
            Purpose = purpose,
            RequestDate = requestDate,
            Status = LoanStatus.Requested
        };
    }

    public void Approve(string? note)
    {
        if (Status != LoanStatus.Requested)
            throw new InvalidOperationException("Only a requested loan can be approved.");
        Status = LoanStatus.Approved;
        Resolution = note;
        base.Update();
    }

    public void Reject(string reason)
    {
        if (Status != LoanStatus.Requested)
            throw new InvalidOperationException("Only a requested loan can be rejected.");
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("A rejection reason is required.", nameof(reason));
        Status = LoanStatus.Rejected;
        Resolution = reason.Trim();
        base.Update();
    }

    public void Cancel()
    {
        if (Status is not (LoanStatus.Requested or LoanStatus.Approved))
            throw new InvalidOperationException("Only a requested or approved loan can be cancelled.");
        Status = LoanStatus.Cancelled;
        base.Update();
    }

    /// <summary>HC256/HC259 — records disbursement (finance/CBS hand-off) and activates repayment.</summary>
    public void Disburse(DateTime date, string? reference)
    {
        if (Status != LoanStatus.Approved)
            throw new InvalidOperationException("Only an approved loan can be disbursed.");
        Status = LoanStatus.Active;
        DisbursedAt = date;
        DisbursementReference = reference;
        base.Update();
    }

    /// <summary>HC258 — the loan is fully repaid/settled.</summary>
    public void Settle(DateTime date)
    {
        if (Status != LoanStatus.Active)
            throw new InvalidOperationException("Only an active loan can be settled.");
        Status = LoanStatus.Settled;
        SettledAt = date;
        base.Update();
    }

    /// <summary>HC257 — the borrower's online service-commitment consent after endorsement/disbursement.</summary>
    public void GiveServiceCommitmentConsent(DateTime date)
    {
        if (Status is not (LoanStatus.Approved or LoanStatus.Active))
            throw new InvalidOperationException("Consent can only be given on an approved or active loan.");
        if (ServiceCommitmentMonths <= 0)
            throw new InvalidOperationException("This loan carries no service commitment.");
        ServiceCommitmentConsentAt = date;
        base.Update();
    }

    /// <summary>HC255 — raises the monthly installment (the remaining schedule is regenerated by the handler).</summary>
    public void SetMonthlyInstallment(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Installment must be positive.", nameof(amount));
        if (amount < MonthlyInstallment)
            throw new InvalidOperationException("The installment can only be increased.");
        MonthlyInstallment = amount;
        base.Update();
    }
}

/// <summary>HC252 — a guarantor backing a loan request (an internal employee or an external person).</summary>
public class LoanGuarantor : BaseEntity
{
    public Guid LoanId { get; private set; }
    /// <summary>Set when the guarantor is a fellow employee; null for an external guarantor.</summary>
    public Guid? GuarantorEmployeeId { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string? IdentificationNumber { get; private set; }
    public string? Relationship { get; private set; }
    public string? PhoneNumber { get; private set; }
    public decimal? GuaranteedAmount { get; private set; }

    private LoanGuarantor() : base() { }

    public static LoanGuarantor Create(Guid loanId, Guid? guarantorEmployeeId, string fullName,
        string? identificationNumber, string? relationship, string? phoneNumber, decimal? guaranteedAmount)
    {
        if (loanId == Guid.Empty) throw new ArgumentException("Loan is required.", nameof(loanId));
        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("Guarantor name is required.", nameof(fullName));
        return new LoanGuarantor
        {
            LoanId = loanId,
            GuarantorEmployeeId = guarantorEmployeeId,
            FullName = fullName.Trim(),
            IdentificationNumber = identificationNumber,
            Relationship = relationship,
            PhoneNumber = phoneNumber,
            GuaranteedAmount = guaranteedAmount
        };
    }
}

/// <summary>HC253 — one amortization row of a loan's repayment schedule.</summary>
public class LoanRepaymentScheduleLine : BaseEntity
{
    public Guid LoanId { get; private set; }
    public int InstallmentNo { get; private set; }
    public DateTime DueDate { get; private set; }
    public decimal PrincipalPortion { get; private set; }
    public decimal InterestPortion { get; private set; }
    public decimal Amount { get; private set; }
    public LoanInstallmentStatus Status { get; private set; } = LoanInstallmentStatus.Pending;
    public DateTime? PaidAt { get; private set; }

    private LoanRepaymentScheduleLine() : base() { }

    public static LoanRepaymentScheduleLine Create(Guid loanId, int installmentNo, DateTime dueDate,
        decimal principalPortion, decimal interestPortion, decimal amount)
    {
        if (loanId == Guid.Empty) throw new ArgumentException("Loan is required.", nameof(loanId));
        return new LoanRepaymentScheduleLine
        {
            LoanId = loanId,
            InstallmentNo = installmentNo,
            DueDate = dueDate,
            PrincipalPortion = principalPortion,
            InterestPortion = interestPortion,
            Amount = amount,
            Status = LoanInstallmentStatus.Pending
        };
    }

    public void MarkPaid(DateTime paidAt)
    {
        Status = LoanInstallmentStatus.Paid;
        PaidAt = paidAt;
        base.Update();
    }
}
