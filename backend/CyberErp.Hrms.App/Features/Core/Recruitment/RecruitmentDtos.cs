using CyberErp.Hrms.Dom.Entities.Core;
using FluentValidation;

namespace CyberErp.Hrms.App.Features.Core.Recruitment
{
    // ---- Hiring requests (HC077–HC083) -------------------------------------------

    public class HiringRequestDto
    {
        public Guid Id { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public Guid OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid PositionClassId { get; set; }
        public string? PositionClassTitle { get; set; }
        public string? JobGradeName { get; set; }
        public int NumberOfPositions { get; set; }
        public string EmploymentType { get; set; } = string.Empty;
        public string Justification { get; set; } = string.Empty;
        public string? JobRequirements { get; set; }
        public DateTime? ExpectedStartDate { get; set; }
        public string? TimelineRemarks { get; set; }
        public decimal EstimatedBudget { get; set; }
        public Guid? WorkforcePlanId { get; set; }
        public string? WorkforcePlanName { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public bool AwaitingWorkflow { get; set; }
        /// <summary>Currently vacant seats for the unit × role (HC082 context).</summary>
        public int VacantSeats { get; set; }
        /// <summary>Positions already covered by non-cancelled requisitions of this request.</summary>
        public int RequisitionedPositions { get; set; }
    }

    public class SaveHiringRequestDto
    {
        public Guid? Id { get; set; }
        public Guid OrganizationUnitId { get; set; }
        public Guid PositionClassId { get; set; }
        public int NumberOfPositions { get; set; } = 1;
        public string EmploymentType { get; set; } = nameof(PlannedEmploymentType.Permanent);
        public string Justification { get; set; } = string.Empty;
        public string? JobRequirements { get; set; }
        public DateTime? ExpectedStartDate { get; set; }
        public string? TimelineRemarks { get; set; }
        public decimal EstimatedBudget { get; set; }
        public Guid? WorkforcePlanId { get; set; }
    }

    public class SaveHiringRequestDtoValidator : AbstractValidator<SaveHiringRequestDto>
    {
        public SaveHiringRequestDtoValidator()
        {
            RuleFor(x => x.OrganizationUnitId).NotEmpty().WithMessage("The requesting directorate/unit is required.");
            RuleFor(x => x.PositionClassId).NotEmpty().WithMessage("The role (position class) is required.");
            RuleFor(x => x.NumberOfPositions).InclusiveBetween(1, 500);
            RuleFor(x => x.EmploymentType)
                .Must(v => Enum.TryParse<PlannedEmploymentType>(v, true, out _))
                .WithMessage("EmploymentType must be Permanent, Contract, Intern or Consultant.");
            RuleFor(x => x.Justification).NotEmpty().MaximumLength(2000);
            RuleFor(x => x.JobRequirements).MaximumLength(2000);
            RuleFor(x => x.TimelineRemarks).MaximumLength(1000);
            RuleFor(x => x.EstimatedBudget).GreaterThanOrEqualTo(0);
        }
    }

    /// <summary>Per-unit recruitment budget/headcount monitor (HC083).</summary>
    public class RecruitmentBudgetRowDto
    {
        public Guid OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public int ApprovedRequests { get; set; }
        public int RequestedPositions { get; set; }
        public decimal EstimatedBudget { get; set; }
        public int OpenRequisitions { get; set; }
    }

    // ---- Job requisitions (HC084–HC088, HC091, HC095) ------------------------------

    /// <summary>One evaluator assigned to a criterion — internal employee or named external.</summary>
    public class CriterionEvaluatorDto
    {
        /// <summary>Employee | ExternalPerson | Organization.</summary>
        public string EvaluatorType { get; set; } = nameof(CriterionEvaluatorType.Employee);
        public Guid? EmployeeId { get; set; }
        /// <summary>External evaluator's name, or the resolved employee name on reads.</summary>
        public string? Name { get; set; }
    }

    public class ScreeningCriterionDto
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsMandatory { get; set; }
        /// <summary>Percentage of the final ranking score — all criteria must total exactly 100.</summary>
        public int Weight { get; set; } = 1;
        /// <summary>The evaluators assigned to score this criterion (any number, mixed kinds).</summary>
        public List<CriterionEvaluatorDto> Evaluators { get; set; } = [];
        /// <summary>The recruitment level the criterion is scored at (e.g. Screening, Interview) — null = all steps.</summary>
        public string? AppliesAtStage { get; set; }
    }

    public class JobRequisitionDto
    {
        public Guid Id { get; set; }
        public string RequisitionNumber { get; set; } = string.Empty;
        public Guid HiringRequestId { get; set; }
        public string? HiringRequestNumber { get; set; }
        public Guid OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid PositionClassId { get; set; }
        public string? PositionClassTitle { get; set; }
        public string? JobGradeName { get; set; }
        public Guid? WorkLocationId { get; set; }
        public string? WorkLocationName { get; set; }
        public int NumberOfPositions { get; set; }
        public string EmploymentType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? MinQualifications { get; set; }
        public int? MinExperienceYears { get; set; }
        public string? Skills { get; set; }
        public Guid? SalaryScaleId { get; set; }
        public decimal? SalaryScaleAmount { get; set; }
        public string PostingChannel { get; set; } = string.Empty;
        public string? PostingText { get; set; }
        public DateTime? OpenFrom { get; set; }
        public DateTime? OpenUntil { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? PostedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public bool AwaitingWorkflow { get; set; }
        public int ApplicationCount { get; set; }
        public List<ScreeningCriterionDto> ScreeningCriteria { get; set; } = [];
    }

    public class SaveJobRequisitionDto
    {
        public Guid? Id { get; set; }
        /// <summary>Required on create — a requisition executes an APPROVED hiring request (HC080).</summary>
        public Guid HiringRequestId { get; set; }
        public int NumberOfPositions { get; set; } = 1;
        public string EmploymentType { get; set; } = nameof(PlannedEmploymentType.Permanent);
        /// <summary>Blank fields default from the hiring request's position class (HC084).</summary>
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? MinQualifications { get; set; }
        public int? MinExperienceYears { get; set; }
        public string? Skills { get; set; }
        public Guid? WorkLocationId { get; set; }
        public Guid? SalaryScaleId { get; set; }
        public List<ScreeningCriterionDto> ScreeningCriteria { get; set; } = [];
    }

    public class SaveJobRequisitionDtoValidator : AbstractValidator<SaveJobRequisitionDto>
    {
        public SaveJobRequisitionDtoValidator()
        {
            RuleFor(x => x.HiringRequestId).NotEmpty().WithMessage("An approved hiring request is required (HC080).");
            RuleFor(x => x.NumberOfPositions).InclusiveBetween(1, 500);
            RuleFor(x => x.EmploymentType)
                .Must(v => Enum.TryParse<PlannedEmploymentType>(v, true, out _))
                .WithMessage("EmploymentType must be Permanent, Contract, Intern or Consultant.");
            RuleFor(x => x.Title).MaximumLength(200);
            RuleFor(x => x.Description).MaximumLength(4000);
            RuleFor(x => x.MinQualifications).MaximumLength(1000);
            RuleFor(x => x.Skills).MaximumLength(1000);
            RuleFor(x => x.MinExperienceYears).GreaterThanOrEqualTo(0).When(x => x.MinExperienceYears.HasValue);
            // Weights are percentages of the final ranking score — they must account for all of it.
            RuleFor(x => x.ScreeningCriteria)
                .Must(list => list.Count == 0 || list.Sum(c => c.Weight) == 100)
                .WithMessage(x => $"Criterion weights must total exactly 100% (currently {x.ScreeningCriteria.Sum(c => c.Weight)}%).");
            RuleForEach(x => x.ScreeningCriteria).ChildRules(c =>
            {
                c.RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
                c.RuleFor(x => x.Weight).InclusiveBetween(1, 100);
                c.RuleFor(x => x.AppliesAtStage)
                    .Must(v => string.IsNullOrEmpty(v) || Enum.TryParse<ApplicationStage>(v, true, out _))
                    .WithMessage("AppliesAtStage must be a pipeline stage (e.g. Screening, Interview) or empty for all steps.");
                // A criterion may carry any number of evaluators; each must be concrete + complete.
                c.RuleFor(x => x.Evaluators)
                    .Must(list => list
                        .Where(e => e.EmployeeId.HasValue)
                        .GroupBy(e => e.EmployeeId!.Value)
                        .All(g => g.Count() == 1))
                    .WithMessage("The same employee is assigned to this criterion more than once.");
                c.RuleForEach(x => x.Evaluators).ChildRules(e =>
                {
                    e.RuleFor(x => x.EvaluatorType)
                        .Must(v => Enum.TryParse<CriterionEvaluatorType>(v, true, out var parsed)
                                   && parsed != CriterionEvaluatorType.None)
                        .WithMessage("EvaluatorType must be Employee, ExternalPerson or Organization.");
                    e.RuleFor(x => x.EmployeeId).NotEmpty()
                        .When(x => string.Equals(x.EvaluatorType, "Employee", StringComparison.OrdinalIgnoreCase))
                        .WithMessage("Select the employee evaluator.");
                    e.RuleFor(x => x.Name).NotEmpty().MaximumLength(300)
                        .When(x => string.Equals(x.EvaluatorType, "ExternalPerson", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(x.EvaluatorType, "Organization", StringComparison.OrdinalIgnoreCase))
                        .WithMessage("Name the external evaluator.");
                });
            });
        }
    }

    public class SetPostingDto
    {
        public Guid Id { get; set; }
        public string PostingChannel { get; set; } = nameof(Dom.Entities.Core.PostingChannel.Internal);
        public string? PostingText { get; set; }
        public DateTime? OpenFrom { get; set; }
        public DateTime? OpenUntil { get; set; }
    }

    // ---- Candidates (HC092–HC097, HC089–HC090) --------------------------------------

    public class CandidateDto
    {
        public Guid Id { get; set; }
        public string CandidateNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string? FatherName { get; set; }
        public string? GrandFatherName { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public string Source { get; set; } = string.Empty;
        public Guid? InternalEmployeeId { get; set; }
        public string? InternalEmployeeName { get; set; }
        public string? EducationSummary { get; set; }
        public string? ExperienceSummary { get; set; }
        public string? SkillsSummary { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? ResumeFileName { get; set; }
        public bool ConsentGiven { get; set; }
        public DateTime? ConsentAt { get; set; }
        public bool IsArchived { get; set; }
        public DateTime? AnonymizedAt { get; set; }
        public bool IsInTalentPool { get; set; }
        public string? TalentPoolNotes { get; set; }
        public int ApplicationCount { get; set; }
        /// <summary>The shared CorePerson record backing this candidate (hire-conversion anchor).</summary>
        public Guid? PersonId { get; set; }
        /// <summary>The employee this candidate became when hired.</summary>
        public Guid? HiredEmployeeId { get; set; }
        /// <summary>Mandatory compliance documents still missing (empty = ready to hire).</summary>
        public List<string> MissingComplianceDocuments { get; set; } = [];
        public bool ComplianceComplete { get; set; }
    }

    public class CandidateDocumentDto
    {
        public Guid Id { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    /// <summary>Converts a candidate into an employee on the SAME person record (no re-entry).</summary>
    public class HireCandidateDto
    {
        public Guid Id { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;
        public DateTime? HireDate { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? SalaryScaleId { get; set; }
        /// <summary>Explicit salary wins; otherwise the salary-scale amount applies.</summary>
        public decimal? Salary { get; set; }
        public string EmploymentNature { get; set; } = nameof(Dom.Entities.Core.EmploymentNature.Permanent);
        /// <summary>Contract length in months — required for a Contract nature.</summary>
        public int? ContractPeriod { get; set; }
        public bool IsProbation { get; set; }
        public DateTime? ProbationEndDate { get; set; }
    }

    public class HireCandidateDtoValidator : AbstractValidator<HireCandidateDto>
    {
        public HireCandidateDtoValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.EmployeeNumber).NotEmpty().MaximumLength(50);
            RuleFor(x => x.EmploymentNature)
                .Must(v => Enum.TryParse<EmploymentNature>(v, true, out _))
                .WithMessage("EmploymentNature must be Permanent or Contract.");
            RuleFor(x => x.ContractPeriod).NotNull().GreaterThan(0)
                .When(x => string.Equals(x.EmploymentNature, "Contract", StringComparison.OrdinalIgnoreCase))
                .WithMessage("A contract hire needs a positive contract period (months).");
            RuleFor(x => x.ProbationEndDate).NotNull()
                .When(x => x.IsProbation)
                .WithMessage("A probation end date is required when probation tracking starts.");
            RuleFor(x => x.Salary).GreaterThanOrEqualTo(0).When(x => x.Salary.HasValue);
        }
    }

    public class SaveCandidateDto
    {
        public Guid? Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? FatherName { get; set; }
        public string? GrandFatherName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public string Source { get; set; } = nameof(CandidateSource.External);
        public Guid? InternalEmployeeId { get; set; }
        public string? EducationSummary { get; set; }
        public string? ExperienceSummary { get; set; }
        public string? SkillsSummary { get; set; }
        public int? YearsOfExperience { get; set; }
        /// <summary>Mandatory data-processing consent (HC097).</summary>
        public bool ConsentGiven { get; set; }
    }

    public class SaveCandidateDtoValidator : AbstractValidator<SaveCandidateDto>
    {
        public SaveCandidateDtoValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.FatherName).MaximumLength(100);
            RuleFor(x => x.GrandFatherName).MaximumLength(100);
            RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
            RuleFor(x => x.PhoneNumber).MaximumLength(50);
            // Grandfather name + gender back the shared CorePerson record (hire-conversion anchor).
            RuleFor(x => x.GrandFatherName).NotEmpty()
                .WithMessage("Grandfather name is required (it backs the person record used at hire).");
            RuleFor(x => x.Gender).NotEmpty()
                .Must(v => Enum.TryParse<Gender>(v, true, out _))
                .WithMessage("Gender must be Male or Female.");
            RuleFor(x => x.Source)
                .Must(v => Enum.TryParse<CandidateSource>(v, true, out _))
                .WithMessage("Source must be External, Internal, JobBoard, SocialMedia, Referral or WalkIn.");
            RuleFor(x => x.ConsentGiven).Equal(true)
                .WithMessage("Data-processing consent is required to record a candidate (HC097).");
            RuleFor(x => x.EducationSummary).MaximumLength(2000);
            RuleFor(x => x.ExperienceSummary).MaximumLength(2000);
            RuleFor(x => x.SkillsSummary).MaximumLength(1000);
            RuleFor(x => x.YearsOfExperience).GreaterThanOrEqualTo(0).When(x => x.YearsOfExperience.HasValue);
        }
    }

    public class SetTalentPoolDto
    {
        public Guid Id { get; set; }
        public bool InPool { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>One ranked internal-matching result for a vacancy (HC090).</summary>
    public class CandidateMatchDto
    {
        public Guid CandidateId { get; set; }
        public string CandidateNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public bool IsInTalentPool { get; set; }
        public int? YearsOfExperience { get; set; }
        public int MatchScore { get; set; }
        public List<string> MatchedSkills { get; set; } = [];
        public bool MeetsExperience { get; set; }
    }

    // ---- Applications (HC098–HC099) ---------------------------------------------------

    /// <summary>One evaluator's score of one criterion (read side, with resolved criterion info).</summary>
    public class CriterionScoreDto
    {
        public Guid CriterionId { get; set; }
        public string? CriterionName { get; set; }
        public bool IsMandatory { get; set; }
        public int Weight { get; set; }
        /// <summary>Display list of the criterion's assigned evaluators ("A, B, …").</summary>
        public string? EvaluatorName { get; set; }
        /// <summary>The recruitment level the criterion is scored at — null = all steps.</summary>
        public string? AppliesAtStage { get; set; }
        public decimal? Score { get; set; }
        public string? Remarks { get; set; }
        public string? ScoredBy { get; set; }
        public DateTime? ScoredAt { get; set; }
    }

    public class ScoreEntryDto
    {
        public Guid CriterionId { get; set; }
        public decimal Score { get; set; }
        public string? Remarks { get; set; }
    }

    /// <summary>Evaluator score sheet for one application — total auto-recomputed from the entries.</summary>
    public class ScoreApplicationDto
    {
        public Guid Id { get; set; }
        public List<ScoreEntryDto> Scores { get; set; } = [];
    }

    public class ScoreApplicationDtoValidator : AbstractValidator<ScoreApplicationDto>
    {
        public ScoreApplicationDtoValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Scores).NotEmpty().WithMessage("Provide at least one criterion score.");
            RuleForEach(x => x.Scores).ChildRules(s =>
            {
                s.RuleFor(x => x.CriterionId).NotEmpty();
                s.RuleFor(x => x.Score).InclusiveBetween(0, 100);
                s.RuleFor(x => x.Remarks).MaximumLength(1000);
            });
        }
    }

    /// <summary>One row of a vacancy's candidate ranking (auto-calculated weighted totals).</summary>
    public class ApplicationRankingRowDto
    {
        public Guid ApplicationId { get; set; }
        public Guid CandidateId { get; set; }
        public string? CandidateNumber { get; set; }
        public string? CandidateName { get; set; }
        public string Stage { get; set; } = string.Empty;
        public decimal? TotalScore { get; set; }
        public int ScoredCriteria { get; set; }
        public int TotalCriteria { get; set; }
        /// <summary>A mandatory criterion scored below 50 — the candidate fails screening.</summary>
        public bool FailsMandatory { get; set; }
        /// <summary>1-based position by weighted total (scored candidates only).</summary>
        public int? Rank { get; set; }
        /// <summary>
        /// Eligible | Waitlisted | Hired | OfferRejected | OutOfContention | FailsMandatory | NotScored.
        /// Only the top-N in-play candidates (N = open positions) are Eligible; a declined/expired
        /// offer removes the candidate from contention, sliding the next one up from the waitlist.
        /// </summary>
        public string? HireEligibility { get; set; }
        public string? LatestOfferStatus { get; set; }
        public List<CriterionScoreDto> Breakdown { get; set; } = [];
    }

    public class ApplicationStageLogDto
    {
        public string Stage { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string? ActedBy { get; set; }
        public DateTime ActedAt { get; set; }
    }

    public class JobApplicationDto
    {
        public Guid Id { get; set; }
        public Guid CandidateId { get; set; }
        public string? CandidateNumber { get; set; }
        public string? CandidateName { get; set; }
        public Guid RequisitionId { get; set; }
        public string? RequisitionNumber { get; set; }
        public string? RequisitionTitle { get; set; }
        public string Stage { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }
        public decimal? ScreeningScore { get; set; }
        public string? ScreeningRemarks { get; set; }
        /// <summary>Total criteria defined on the vacancy.</summary>
        public int TotalCriteriaCount { get; set; }
        /// <summary>
        /// Criteria scoreable at the application's CURRENT stage: global criteria (no level) always
        /// count; level-scoped criteria count only while the application sits at that level. Drives
        /// the visibility of the score button in the pipeline UI.
        /// </summary>
        public int ScoreableCriteriaCount { get; set; }
        public List<ApplicationStageLogDto> StageLog { get; set; } = [];
        /// <summary>The requisition's criteria merged with this application's scores (score sheet).</summary>
        public List<CriterionScoreDto> CriterionScores { get; set; } = [];
    }

    public class CreateJobApplicationDto
    {
        public Guid CandidateId { get; set; }
        public Guid RequisitionId { get; set; }
        public DateTime? AppliedAt { get; set; }
    }

    public class CreateJobApplicationDtoValidator : AbstractValidator<CreateJobApplicationDto>
    {
        public CreateJobApplicationDtoValidator()
        {
            RuleFor(x => x.CandidateId).NotEmpty();
            RuleFor(x => x.RequisitionId).NotEmpty();
        }
    }

    public class MoveApplicationStageDto
    {
        public Guid Id { get; set; }
        public string Stage { get; set; } = string.Empty;
        public string? Note { get; set; }
        /// <summary>Screening outcome, recorded together with the move (HC099).</summary>
        public decimal? ScreeningScore { get; set; }
        public string? ScreeningRemarks { get; set; }
    }

    public class MoveApplicationStageDtoValidator : AbstractValidator<MoveApplicationStageDto>
    {
        public MoveApplicationStageDtoValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Stage)
                .Must(v => Enum.TryParse<ApplicationStage>(v, true, out _))
                .WithMessage("Unknown application stage.");
            RuleFor(x => x.Note).MaximumLength(1000);
            RuleFor(x => x.ScreeningScore).InclusiveBetween(0, 100).When(x => x.ScreeningScore.HasValue);
            RuleFor(x => x.ScreeningRemarks).MaximumLength(2000);
        }
    }
}
