using CyberErp.Hrms.Dom.Entities;

namespace CyberErp.Hrms.Dom.Entities.Core;

/// <summary>Kind of approved medical service provider (HC238).</summary>
public enum MedicalProviderType
{
    Hospital = 0,
    Clinic = 1,
    Laboratory = 2,
    Pharmacy = 3,
    Other = 4
}

/// <summary>Lifecycle of a credit medical service contract (HC236).</summary>
public enum MedicalContractStatus
{
    Active = 0,
    Expired = 1,
    Terminated = 2
}

/// <summary>
/// HC238 — an approved medical service provider (hospital, clinic, laboratory, pharmacy…) with its
/// contact details and specialization. Claims and credit contracts reference it.
/// </summary>
public class MedicalProvider : BaseEntity, IAggregateRoot, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    public MedicalProviderType ProviderType { get; private set; }
    public string? Specialization { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Email { get; private set; }
    public string? Address { get; private set; }
    public bool IsActive { get; private set; } = true;

    private MedicalProvider() : base() { }

    public static MedicalProvider Create(string name, MedicalProviderType type, string? specialization,
        string? phone, string? email, string? address, bool isActive = true)
    {
        Guard(name);
        return new MedicalProvider
        {
            Name = name.Trim(),
            ProviderType = type,
            Specialization = specialization,
            PhoneNumber = phone,
            Email = email,
            Address = address,
            IsActive = isActive
        };
    }

    public void Update(string name, MedicalProviderType type, string? specialization,
        string? phone, string? email, string? address, bool isActive)
    {
        Guard(name);
        Name = name.Trim();
        ProviderType = type;
        Specialization = specialization;
        PhoneNumber = phone;
        Email = email;
        Address = address;
        IsActive = isActive;
        base.Update();
    }

    private static void Guard(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Provider name cannot be empty.", nameof(name));
    }
}

/// <summary>
/// HC235 — a medical (health) coverage plan: what it reimburses (annual limit, coverage percent) and
/// whether dependents are covered. The premium/contribution is modelled separately as a benefit plan
/// (<see cref="BenefitPlan"/>), optionally linked here.
/// </summary>
public class MedicalPlan : BaseEntity, IAggregateRoot, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    /// <summary>Annual coverage ceiling per beneficiary; null = uncapped.</summary>
    public decimal? AnnualCoverageLimit { get; private set; }
    /// <summary>Share of an eligible claim the plan reimburses (0–100).</summary>
    public decimal CoveragePercent { get; private set; } = 100m;
    public bool CoversDependents { get; private set; } = true;
    /// <summary>Optional link to the benefit plan carrying the premium/contribution.</summary>
    public Guid? BenefitPlanId { get; private set; }
    public bool IsActive { get; private set; } = true;

    private MedicalPlan() : base() { }

    public static MedicalPlan Create(string name, string? description, decimal? annualLimit,
        decimal coveragePercent, bool coversDependents, Guid? benefitPlanId, bool isActive = true)
    {
        Guard(name, annualLimit, coveragePercent);
        return new MedicalPlan
        {
            Name = name.Trim(),
            Description = description,
            AnnualCoverageLimit = annualLimit,
            CoveragePercent = coveragePercent,
            CoversDependents = coversDependents,
            BenefitPlanId = benefitPlanId,
            IsActive = isActive
        };
    }

    public void Update(string name, string? description, decimal? annualLimit,
        decimal coveragePercent, bool coversDependents, Guid? benefitPlanId, bool isActive)
    {
        Guard(name, annualLimit, coveragePercent);
        Name = name.Trim();
        Description = description;
        AnnualCoverageLimit = annualLimit;
        CoveragePercent = coveragePercent;
        CoversDependents = coversDependents;
        BenefitPlanId = benefitPlanId;
        IsActive = isActive;
        base.Update();
    }

    private static void Guard(string name, decimal? annualLimit, decimal coveragePercent)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Plan name cannot be empty.", nameof(name));
        if (annualLimit is < 0)
            throw new ArgumentException("Coverage limit cannot be negative.", nameof(annualLimit));
        if (coveragePercent is < 0 or > 100)
            throw new ArgumentException("Coverage percent must be between 0 and 100.", nameof(coveragePercent));
    }
}

/// <summary>
/// HC236 — a credit medical service agreement with a provider: its terms, credit limit and renewal
/// dates. Approved claims against a contracted provider draw on the agreement.
/// </summary>
public class MedicalServiceContract : BaseEntity, IAggregateRoot, IAuditable
{
    public Guid MedicalProviderId { get; private set; }
    public string? ContractNumber { get; private set; }
    public string? Terms { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? RenewalDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public decimal? CreditLimit { get; private set; }
    public MedicalContractStatus Status { get; private set; } = MedicalContractStatus.Active;
    public string? Notes { get; private set; }

    private MedicalServiceContract() : base() { }

    public static MedicalServiceContract Create(Guid providerId, string? contractNumber, string? terms,
        DateTime startDate, DateTime? renewalDate, DateTime? endDate, decimal? creditLimit,
        MedicalContractStatus status, string? notes)
    {
        Guard(providerId, startDate, endDate, creditLimit);
        return new MedicalServiceContract
        {
            MedicalProviderId = providerId,
            ContractNumber = contractNumber,
            Terms = terms,
            StartDate = startDate,
            RenewalDate = renewalDate,
            EndDate = endDate,
            CreditLimit = creditLimit,
            Status = status,
            Notes = notes
        };
    }

    public void Update(Guid providerId, string? contractNumber, string? terms,
        DateTime startDate, DateTime? renewalDate, DateTime? endDate, decimal? creditLimit,
        MedicalContractStatus status, string? notes)
    {
        Guard(providerId, startDate, endDate, creditLimit);
        MedicalProviderId = providerId;
        ContractNumber = contractNumber;
        Terms = terms;
        StartDate = startDate;
        RenewalDate = renewalDate;
        EndDate = endDate;
        CreditLimit = creditLimit;
        Status = status;
        Notes = notes;
        base.Update();
    }

    private static void Guard(Guid providerId, DateTime startDate, DateTime? endDate, decimal? creditLimit)
    {
        if (providerId == Guid.Empty)
            throw new ArgumentException("Provider is required.", nameof(providerId));
        if (endDate.HasValue && endDate.Value < startDate)
            throw new ArgumentException("End date cannot precede the start date.", nameof(endDate));
        if (creditLimit is < 0)
            throw new ArgumentException("Credit limit cannot be negative.", nameof(creditLimit));
    }
}
