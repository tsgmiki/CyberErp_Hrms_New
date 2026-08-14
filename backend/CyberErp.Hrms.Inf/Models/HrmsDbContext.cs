using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using CyberErp.Hrms.Dom.Entities.Core;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using CyberErp.Hrms.Inf.Models.EntityConfiguration;
using NodaTime;

namespace CyberErp.Hrms.Inf.Models;

public class HrmsDbContext : MultiTenantDbContext
{
    public HrmsDbContext(DbContextOptions<HrmsDbContext> options, IMultiTenantContextAccessor<AppTenantInfo>? tenantContextAccessor = null)
        : base(tenantContextAccessor, options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var basePath = AppContext.BaseDirectory;
            var appsettingsPath = Path.Combine(basePath, "CyberErp.Hrms.Api", "appsettings.json");
            if (!File.Exists(appsettingsPath))
                appsettingsPath = Path.Combine(basePath, "appsettings.json");

            if (File.Exists(appsettingsPath))
            {
                var json = File.ReadAllText(appsettingsPath);
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.TryGetProperty("ConnectionStrings", out var connStrings) &&
                    connStrings.TryGetProperty("DefaultConnection", out var defaultConn))
                {
                    var conn = defaultConn.GetString();
                    if (!string.IsNullOrEmpty(conn))
                    {
                        optionsBuilder.UseSqlServer(conn, b =>
                        {
                            b.MigrationsAssembly("CyberErp.Hrms.Inf");
                            b.CommandTimeout(60);
                        });
                    }
                }
            }
        }
    }

    public DbSet<User> User { get; set; }
    public DbSet<Tenant> Tenant { get; set; }
    public DbSet<SubscriptionPlan> SubscriptionPlan { get; set; }

    // ---- Platform layer ported from the SRMS schema ----
    public DbSet<Organization> Organization { get; set; }
    public DbSet<OrganizationSubscription> OrganizationSubscription { get; set; }
    public DbSet<SubscriptionPlanModule> SubscriptionPlanModule { get; set; }
    public DbSet<TenantSubscriptionAddOn> TenantSubscriptionAddOn { get; set; }
    public DbSet<LoginTrail> LoginTrail { get; set; }
    public DbSet<Setting> Setting { get; set; }
    public DbSet<UserPreference> UserPreference { get; set; }

    // ---- Tenant-scoped authorization (SRMS phase 2) ----
    public DbSet<TenantRole> TenantRole { get; set; }
    public DbSet<TenantOperation> TenantOperation { get; set; }
    public DbSet<TenantRolePermission> TenantRolePermission { get; set; }
    public DbSet<TenantUser> TenantUser { get; set; }
    public DbSet<TenantUserRole> TenantUserRole { get; set; }
    public DbSet<TenantSubSystem> TenantSubSystem { get; set; }
    public DbSet<TenantSubscription> TenantSubscription { get; set; }

    // Organizational Structure (HRMS §3.1)
    public DbSet<OrganizationUnit> OrganizationUnit { get; set; }
    public DbSet<Position> Position { get; set; }
    public DbSet<PositionClass> PositionClass { get; set; }
    public DbSet<JobGrade> JobGrade { get; set; }
    public DbSet<Step> Step { get; set; }
    public DbSet<SalaryScale> SalaryScale { get; set; }
    public DbSet<FiscalYear> FiscalYear { get; set; }
    public DbSet<AnnualLeaveSetting> AnnualLeaveSetting { get; set; }
    public DbSet<LeaveType> LeaveType { get; set; }
    public DbSet<Holiday> Holiday { get; set; }
    public DbSet<LeaveRequest> LeaveRequest { get; set; }
    public DbSet<LeaveRequestLine> LeaveRequestLine { get; set; }
    public DbSet<WorkWeekConfiguration> WorkWeekConfiguration { get; set; }
    public DbSet<Report> Report { get; set; }
    public DbSet<ReportField> ReportField { get; set; }
    public DbSet<ReportFieldOutput> ReportFieldOutput { get; set; }
    public DbSet<SavedReportFilter> SavedReportFilter { get; set; }
    public DbSet<ReportRun> ReportRun { get; set; }
    public DbSet<ReportSchedule> ReportSchedule { get; set; }
    public DbSet<ReportScheduleRecipient> ReportScheduleRecipient { get; set; }
    public DbSet<ReportScheduleFieldValue> ReportScheduleFieldValue { get; set; }
    public DbSet<ReportScheduleFieldOutput> ReportScheduleFieldOutput { get; set; }
    public DbSet<ReportRunRecipient> ReportRunRecipient { get; set; }
    public DbSet<ReportRestriction> ReportRestriction { get; set; }
    public DbSet<AnnualLeaveHeader> AnnualLeaveHeader { get; set; }
    public DbSet<AnnualLeaveDetail> AnnualLeaveDetail { get; set; }
    public DbSet<AnnualLeaveReturn> AnnualLeaveReturn { get; set; }
    public DbSet<LeaveBalance> LeaveBalance { get; set; }
    public DbSet<LeaveBalanceTransaction> LeaveBalanceTransaction { get; set; }
    public DbSet<JobCategory> JobCategory { get; set; }
    public DbSet<WorkLocation> WorkLocation { get; set; }
    // Generic, centralized lookup system (global reference data — see LookupCategory).
    public DbSet<LookupCategory> LookupCategory { get; set; }
    public DbSet<LookupCategoryList> LookupCategoryList { get; set; }

    // Career Development §3.7.A — Succession Planning (HC148–HC160).
    public DbSet<CriticalPosition> CriticalPosition { get; set; }
    public DbSet<TalentReview> TalentReview { get; set; }
    public DbSet<TalentAssessment> TalentAssessment { get; set; }
    public DbSet<TalentRating> TalentRating { get; set; }
    public DbSet<SuccessionPlan> SuccessionPlan { get; set; }
    public DbSet<SuccessionCandidate> SuccessionCandidate { get; set; }
    public DbSet<SuccessionDevelopmentAction> SuccessionDevelopmentAction { get; set; }
    public DbSet<KnowledgeTransfer> KnowledgeTransfer { get; set; }

    // Career Development §3.7.B — Career Path (HC161–HC169).
    public DbSet<CareerPath> CareerPath { get; set; }
    public DbSet<CareerPathStep> CareerPathStep { get; set; }
    public DbSet<CareerPathStepCompetency> CareerPathStepCompetency { get; set; }
    public DbSet<EmployeeCareerPath> EmployeeCareerPath { get; set; }
    public DbSet<EmployeeCareerPathStepProgress> EmployeeCareerPathStepProgress { get; set; }
    public DbSet<Mentorship> Mentorship { get; set; }
    public DbSet<CareerPathChangeRequest> CareerPathChangeRequest { get; set; }

    // Multi-Branch Organizational Structure
    public DbSet<Branch> Branch { get; set; }
    public DbSet<AuditLog> AuditLog { get; set; }

    // Employee Data Management (HRMS §3.2)
    public DbSet<Person> Person { get; set; }
    public DbSet<Employee> Employee { get; set; }
    public DbSet<EmployeeEducation> EmployeeEducation { get; set; }
    public DbSet<EmployeeExperience> EmployeeExperience { get; set; }
    public DbSet<EmployeeDependent> EmployeeDependent { get; set; }
    public DbSet<EmployeeFieldDefinition> EmployeeFieldDefinition { get; set; }
    public DbSet<EmployeeFieldValue> EmployeeFieldValue { get; set; }
    public DbSet<EmployeeDocument> EmployeeDocument { get; set; }
    public DbSet<EmployeeMovement> EmployeeMovement { get; set; }
    public DbSet<DisciplinaryMeasure> DisciplinaryMeasure { get; set; }
    public DbSet<EmployeeTermination> EmployeeTermination { get; set; }
    public DbSet<TerminationClearance> TerminationClearance { get; set; }
    public DbSet<ClearanceDepartment> ClearanceDepartment { get; set; }
    public DbSet<ClearanceDepartmentApprover> ClearanceDepartmentApprover { get; set; }
    public DbSet<DynamicForm> DynamicForm { get; set; }
    public DbSet<DynamicFormField> DynamicFormField { get; set; }
    public DbSet<DynamicFormRecord> DynamicFormRecord { get; set; }
    // Performance Management (HC118–HC147) — Phase A config
    public DbSet<RatingScale> RatingScale { get; set; }
    public DbSet<RatingScaleLevel> RatingScaleLevel { get; set; }
    public DbSet<CompetencyCategory> CompetencyCategory { get; set; }
    public DbSet<Competency> Competency { get; set; }
    public DbSet<PositionCompetency> PositionCompetency { get; set; }
    public DbSet<ReviewCycle> ReviewCycle { get; set; }
    public DbSet<AppraisalTemplate> AppraisalTemplate { get; set; }
    public DbSet<OrganizationalObjective> OrganizationalObjective { get; set; }
    public DbSet<EmployeeGoal> EmployeeGoal { get; set; }
    public DbSet<GoalActionItem> GoalActionItem { get; set; }
    public DbSet<Appraisal> Appraisal { get; set; }
    public DbSet<AppraisalGoal> AppraisalGoal { get; set; }
    public DbSet<AppraisalCompetency> AppraisalCompetency { get; set; }
    public DbSet<AppraisalPeerReview> AppraisalPeerReview { get; set; }
    public DbSet<CalibrationSession> CalibrationSession { get; set; }
    public DbSet<CalibrationItem> CalibrationItem { get; set; }
    public DbSet<PerformanceHistory> PerformanceHistory { get; set; }
    public DbSet<IndividualDevelopmentPlan> IndividualDevelopmentPlan { get; set; }
    public DbSet<DevelopmentAction> DevelopmentAction { get; set; }
    public DbSet<PerformanceImprovementPlan> PerformanceImprovementPlan { get; set; }
    public DbSet<PipObjective> PipObjective { get; set; }
    public DbSet<Achievement> Achievement { get; set; }
    public DbSet<RecognitionBadge> RecognitionBadge { get; set; }
    public DbSet<EmployeeRecognition> EmployeeRecognition { get; set; }
    public DbSet<AwardCategory> AwardCategory { get; set; }
    public DbSet<RecognitionProgram> RecognitionProgram { get; set; }
    public DbSet<AllowanceType> AllowanceType { get; set; }
    public DbSet<EmployeeAllowance> EmployeeAllowance { get; set; }
    public DbSet<SalaryRevision> SalaryRevision { get; set; }
    public DbSet<SalaryRevisionLine> SalaryRevisionLine { get; set; }
    public DbSet<SalaryRevisionBand> SalaryRevisionBand { get; set; }
    public DbSet<SalaryIncrementPolicy> SalaryIncrementPolicy { get; set; }
    public DbSet<BenefitPlan> BenefitPlan { get; set; }
    public DbSet<EmployeeBenefitEnrollment> EmployeeBenefitEnrollment { get; set; }
    public DbSet<TaxBracket> TaxBracket { get; set; }
    public DbSet<CompensationRequest> CompensationRequest { get; set; }
    public DbSet<ProfileChangeRequest> ProfileChangeRequest { get; set; }
    public DbSet<MedicalProvider> MedicalProvider { get; set; }
    public DbSet<MedicalPlan> MedicalPlan { get; set; }
    public DbSet<MedicalServiceContract> MedicalServiceContract { get; set; }
    public DbSet<MedicalEnrollment> MedicalEnrollment { get; set; }
    public DbSet<MedicalBeneficiary> MedicalBeneficiary { get; set; }
    public DbSet<MedicalClaim> MedicalClaim { get; set; }
    public DbSet<MedicalClaimAttachment> MedicalClaimAttachment { get; set; }
    public DbSet<InsurancePolicy> InsurancePolicy { get; set; }
    public DbSet<InsurancePremiumSchedule> InsurancePremiumSchedule { get; set; }
    public DbSet<InsuranceClaim> InsuranceClaim { get; set; }
    public DbSet<InsuranceClaimAttachment> InsuranceClaimAttachment { get; set; }
    public DbSet<LoanType> LoanType { get; set; }
    public DbSet<Loan> Loan { get; set; }
    public DbSet<LoanGuarantor> LoanGuarantor { get; set; }
    public DbSet<LoanRepaymentScheduleLine> LoanRepaymentScheduleLine { get; set; }
    public DbSet<PerDiemRate> PerDiemRate { get; set; }
    public DbSet<TripBudget> TripBudget { get; set; }
    public DbSet<TripRequest> TripRequest { get; set; }
    public DbSet<TripExpense> TripExpense { get; set; }
    public DbSet<EmployeeGuarantee> EmployeeGuarantee { get; set; }
    public DbSet<OtherLeaveSetting> OtherLeaveSetting { get; set; }
    public DbSet<OtherLeaveHeader> OtherLeaveHeader { get; set; }
    public DbSet<OtherLeaveDetail> OtherLeaveDetail { get; set; }
    public DbSet<OtherLeaveAttachment> OtherLeaveAttachment { get; set; }
    public DbSet<RewardNomination> RewardNomination { get; set; }
    public DbSet<RewardPointsTransaction> RewardPointsTransaction { get; set; }
    public DbSet<RewardDisbursement> RewardDisbursement { get; set; }
    public DbSet<TrainingCategory> TrainingCategory { get; set; }
    public DbSet<TrainingCourse> TrainingCourse { get; set; }
    public DbSet<TrainingNeed> TrainingNeed { get; set; }
    public DbSet<TrainingSession> TrainingSession { get; set; }
    public DbSet<TrainingEnrollment> TrainingEnrollment { get; set; }
    public DbSet<TrainingBudget> TrainingBudget { get; set; }
    public DbSet<LearningPath> LearningPath { get; set; }
    public DbSet<LearningPathStep> LearningPathStep { get; set; }
    public DbSet<EmployeeTrainingCertificate> EmployeeTrainingCertificate { get; set; }
    public DbSet<TrainingProviderPayment> TrainingProviderPayment { get; set; }
    public DbSet<LearningCommunity> LearningCommunity { get; set; }
    public DbSet<LearningCommunityMember> LearningCommunityMember { get; set; }
    public DbSet<LearningCommunityPost> LearningCommunityPost { get; set; }
    public DbSet<CommunityPostReaction> CommunityPostReaction { get; set; }
    public DbSet<Suggestion> Suggestion { get; set; }
    public DbSet<Grievance> Grievance { get; set; }
    public DbSet<GrievanceNote> GrievanceNote { get; set; }
    public DbSet<Announcement> Announcement { get; set; }
    public DbSet<CompanyAsset> CompanyAsset { get; set; }
    public DbSet<TerminationAssetRecovery> TerminationAssetRecovery { get; set; }
    public DbSet<ExitQuestionnaire> ExitQuestionnaire { get; set; }
    public DbSet<ExitInterview> ExitInterview { get; set; }
    public DbSet<TerminationSettlement> TerminationSettlement { get; set; }
    public DbSet<SettlementLine> SettlementLine { get; set; }
    public DbSet<Survey> Survey { get; set; }
    public DbSet<SurveyResponse> SurveyResponse { get; set; }
    public DbSet<SurveyCompletion> SurveyCompletion { get; set; }
    public DbSet<AppraisalAppeal> AppraisalAppeal { get; set; }
    public DbSet<WorkforcePlan> WorkforcePlan { get; set; }
    public DbSet<WorkforcePlanLine> WorkforcePlanLine { get; set; }
    public DbSet<HiringRequest> HiringRequest { get; set; }
    public DbSet<JobRequisition> JobRequisition { get; set; }
    public DbSet<RequisitionScreeningCriterion> RequisitionScreeningCriterion { get; set; }
    public DbSet<CriterionEvaluator> CriterionEvaluator { get; set; }
    public DbSet<Candidate> Candidate { get; set; }
    public DbSet<JobApplication> JobApplication { get; set; }
    public DbSet<JobApplicationStageLog> JobApplicationStageLog { get; set; }
    public DbSet<ApplicationCriterionScore> ApplicationCriterionScore { get; set; }
    public DbSet<CandidateDocument> CandidateDocument { get; set; }
    public DbSet<Interview> Interview { get; set; }
    public DbSet<InterviewPanelist> InterviewPanelist { get; set; }
    public DbSet<InterviewFeedback> InterviewFeedback { get; set; }
    public DbSet<JobOffer> JobOffer { get; set; }
    public DbSet<NumberSequence> NumberSequence { get; set; }
    public DbSet<Role> Role { get; set; }
    public DbSet<UserRole> UserRole { get; set; }
    public DbSet<Module> Module { get; set; }
    public DbSet<Operation> Operation { get; set; }
    public DbSet<Subsystem> Subsystem { get; set; }
    /// <summary>Core.Notification — Home-owned portal alerts; HRMS only writes (ExcludeFromMigrations).</summary>
    public DbSet<CoreNotification> CoreNotification { get; set; }
    public DbSet<WorkflowDefinition> WorkflowDefinition { get; set; }
    public DbSet<WorkflowStep> WorkflowStep { get; set; }
    public DbSet<WorkflowStepApprover> WorkflowStepApprover { get; set; }
    public DbSet<WorkflowInstance> WorkflowInstance { get; set; }
    public DbSet<WorkflowActionLog> WorkflowActionLog { get; set; }
    public DbSet<DocumentTemplate> DocumentTemplate { get; set; }
    public DbSet<OfferLetterTemplate> OfferLetterTemplate { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("Core");

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.ClrType.GetProperties())
            {
                if (property.PropertyType == typeof(Guid))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property(property.Name)
                        .HasColumnType("uniqueidentifier");
                }
                else if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(Instant))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property(property.Name)
                        .HasColumnType("datetime2(3)");
                }
                else if (property.PropertyType == typeof(DateTime?) || property.PropertyType == typeof(Instant?))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property(property.Name)
                        .HasColumnType("datetime2(7)");
                }
                else if (property.PropertyType == typeof(byte[]) && property.Name == "RowVersion")
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property(property.Name)
                        .HasColumnType("varbinary(8)")
                        .IsConcurrencyToken()
                        .IsRequired();
                }
            }
        }

        /*
         * TenantId is stored as a uniqueidentifier — the SRMS platform type — while staying a string
         * in the CLR (2026-08-14, logic.md §12.14).
         *
         * WHY A CONVERTER RATHER THAN RETYPING THE PROPERTY
         * -------------------------------------------------
         * `TenantId` is declared once on BaseEntity and carried by 202 tables. It is also the
         * Finbuckle discriminator, and Finbuckle's own ITenantInfo.Id is a STRING — so retyping it to
         * Guid would mean converting at that boundary anyway, plus touching the repository filter,
         * every aggregate-child propagation, the Home portal's IHasTenant and every SQL script. The
         * column type is what had to match; the CLR type never did.
         *
         * Every query use of TenantId is a simple equality (checked: the repository filter, Home's
         * query filters, PortalNotifier, LoginRepository), which a value converter translates
         * cleanly. The `string.IsNullOrEmpty(x.TenantId)` checks scattered through the aggregate
         * handlers all run IN MEMORY, so they are unaffected.
         *
         * ⚠️ The empty string maps to Guid.Empty, and back. Nineteen rows legitimately carry a blank
         * TenantId — the global lookup tables, Organization, Setting — and Guid.Parse would throw on
         * every one of them. Round-tripping through Guid.Empty keeps `IsNullOrEmpty` checks working
         * exactly as before.
         */
        var tenantIdConverter = new ValueConverter<string, Guid>(
            v => string.IsNullOrWhiteSpace(v) ? Guid.Empty : Guid.Parse(v),
            v => v == Guid.Empty ? string.Empty : v.ToString());

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // ⚠️ Ask EF what it MAPPED, not reflection. TenantSubscription declares its own
            // `Guid TenantId` — a real foreign key — which SHADOWS BaseEntity's string one, so
            // Type.GetProperty("TenantId") finds two and throws "Ambiguous match found". Its column
            // is already a uniqueidentifier and must not be converted twice.
            // ⚠️ LoginTrail and UserPreference stay nvarchar — because SRMS itself types them that
            // way. It has TenantId as uniqueidentifier on seven tables and nvarchar on these two;
            // the inconsistency is in the source schema, not here, and matching it is the point of
            // this exercise. If SRMS is ever made consistent, delete this exclusion and the
            // companion migration rather than working around it.
            var tenantId = entityType.FindProperty("TenantId");
            var name = entityType.ClrType.Name;
            if (tenantId is not null && tenantId.ClrType == typeof(string)
                && name is not ("LoginTrail" or "UserPreference"))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property("TenantId")
                    .HasConversion(tenantIdConverter)
                    .HasColumnType("uniqueidentifier");
            }
        }

        var instantConverter = new ValueConverter<Instant, DateTime>(
            v => v.ToDateTimeUtc(),
            v => Instant.FromDateTimeUtc(DateTime.SpecifyKind(v, DateTimeKind.Utc)));

        var nullableInstantConverter = new ValueConverter<Instant?, DateTime?>(
            v => v.HasValue ? v.Value.ToDateTimeUtc() : null,
            v => v.HasValue ? Instant.FromDateTimeUtc(DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)) : null);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.ClrType.GetProperties().Where(p => p.PropertyType == typeof(Instant)))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(property.Name)
                    .HasConversion(instantConverter);
            }

            foreach (var property in entityType.ClrType.GetProperties().Where(p => p.PropertyType == typeof(Instant?)))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(property.Name)
                    .HasConversion(nullableInstantConverter);
            }
        }

        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new TenantConfiguration());
        modelBuilder.ApplyConfiguration(new SubscriptionPlanConfiguration());
        modelBuilder.ApplyConfiguration(new OrganizationConfiguration());
        modelBuilder.ApplyConfiguration(new OrganizationSubscriptionConfiguration());
        modelBuilder.ApplyConfiguration(new SubscriptionPlanModuleConfiguration());
        modelBuilder.ApplyConfiguration(new TenantSubscriptionAddOnConfiguration());
        modelBuilder.ApplyConfiguration(new LoginTrailConfiguration());
        modelBuilder.ApplyConfiguration(new SettingConfiguration());
        modelBuilder.ApplyConfiguration(new UserPreferenceConfiguration());
        modelBuilder.ApplyConfiguration(new TenantRoleConfiguration());
        modelBuilder.ApplyConfiguration(new TenantOperationConfiguration());
        modelBuilder.ApplyConfiguration(new TenantRolePermissionConfiguration());
        modelBuilder.ApplyConfiguration(new TenantUserConfiguration());
        modelBuilder.ApplyConfiguration(new TenantUserRoleConfiguration());
        modelBuilder.ApplyConfiguration(new TenantSubSystemConfiguration());
        modelBuilder.ApplyConfiguration(new TenantSubscriptionConfiguration());

        // Organizational Structure (HRMS §3.1)
        modelBuilder.ApplyConfiguration(new JobGradeConfiguration());
        modelBuilder.ApplyConfiguration(new StepConfiguration());
        modelBuilder.ApplyConfiguration(new SalaryScaleConfiguration());
        modelBuilder.ApplyConfiguration(new FiscalYearConfiguration());
        modelBuilder.ApplyConfiguration(new AnnualLeaveSettingConfiguration());
        modelBuilder.ApplyConfiguration(new LeaveTypeConfiguration());
        modelBuilder.ApplyConfiguration(new HolidayConfiguration());
        modelBuilder.ApplyConfiguration(new LeaveRequestConfiguration());
        modelBuilder.ApplyConfiguration(new LeaveRequestLineConfiguration());
        modelBuilder.ApplyConfiguration(new WorkWeekConfigurationConfiguration());
        modelBuilder.ApplyConfiguration(new ReportConfiguration());
        modelBuilder.ApplyConfiguration(new ReportFieldConfiguration());
        modelBuilder.ApplyConfiguration(new ReportFieldOutputConfiguration());
        modelBuilder.ApplyConfiguration(new SavedReportFilterConfiguration());
        modelBuilder.ApplyConfiguration(new ReportRunConfiguration());
        modelBuilder.ApplyConfiguration(new ReportRunRecipientConfiguration());
        modelBuilder.ApplyConfiguration(new ReportScheduleConfiguration());
        modelBuilder.ApplyConfiguration(new ReportScheduleRecipientConfiguration());
        modelBuilder.ApplyConfiguration(new ReportScheduleFieldValueConfiguration());
        modelBuilder.ApplyConfiguration(new ReportScheduleFieldOutputConfiguration());
        modelBuilder.ApplyConfiguration(new ReportRestrictionConfiguration());
        modelBuilder.ApplyConfiguration(new AnnualLeaveHeaderConfiguration());
        modelBuilder.ApplyConfiguration(new AnnualLeaveDetailConfiguration());
        modelBuilder.ApplyConfiguration(new AnnualLeaveReturnConfiguration());
        modelBuilder.ApplyConfiguration(new LeaveBalanceConfiguration());
        modelBuilder.ApplyConfiguration(new LeaveBalanceTransactionConfiguration());
        modelBuilder.ApplyConfiguration(new JobCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new LookupCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new LookupCategoryListConfiguration());
        // Career Development §3.7.A — Succession Planning.
        modelBuilder.ApplyConfiguration(new CriticalPositionConfiguration());
        modelBuilder.ApplyConfiguration(new TalentReviewConfiguration());
        modelBuilder.ApplyConfiguration(new TalentAssessmentConfiguration());
        modelBuilder.ApplyConfiguration(new TalentRatingConfiguration());
        modelBuilder.ApplyConfiguration(new SuccessionPlanConfiguration());
        modelBuilder.ApplyConfiguration(new SuccessionCandidateConfiguration());
        modelBuilder.ApplyConfiguration(new SuccessionDevelopmentActionConfiguration());
        modelBuilder.ApplyConfiguration(new KnowledgeTransferConfiguration());
        // Career Development §3.7.B — Career Path.
        modelBuilder.ApplyConfiguration(new CareerPathConfiguration());
        modelBuilder.ApplyConfiguration(new CareerPathStepConfiguration());
        modelBuilder.ApplyConfiguration(new CareerPathStepCompetencyConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeCareerPathConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeCareerPathStepProgressConfiguration());
        modelBuilder.ApplyConfiguration(new MentorshipConfiguration());
        modelBuilder.ApplyConfiguration(new CareerPathChangeRequestConfiguration());
        modelBuilder.ApplyConfiguration(new WorkLocationConfiguration());
        modelBuilder.ApplyConfiguration(new OrganizationUnitConfiguration());
        modelBuilder.ApplyConfiguration(new PositionClassConfiguration());
        modelBuilder.ApplyConfiguration(new PositionConfiguration());

        // Multi-Branch Organizational Structure
        modelBuilder.ApplyConfiguration(new BranchConfiguration());
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());

        // Employee Data Management (HRMS §3.2)
        modelBuilder.ApplyConfiguration(new PersonConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeEducationConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeExperienceConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeDependentConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeFieldDefinitionConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeFieldValueConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeDocumentConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeMovementConfiguration());
        modelBuilder.ApplyConfiguration(new DisciplinaryMeasureConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeTerminationConfiguration());
        modelBuilder.ApplyConfiguration(new TerminationClearanceConfiguration());
        modelBuilder.ApplyConfiguration(new ClearanceDepartmentConfiguration());
        modelBuilder.ApplyConfiguration(new ClearanceDepartmentApproverConfiguration());
        modelBuilder.ApplyConfiguration(new DynamicFormConfiguration());
        modelBuilder.ApplyConfiguration(new DynamicFormFieldConfiguration());
        modelBuilder.ApplyConfiguration(new DynamicFormRecordConfiguration());
        modelBuilder.ApplyConfiguration(new RatingScaleConfiguration());
        modelBuilder.ApplyConfiguration(new RatingScaleLevelConfiguration());
        modelBuilder.ApplyConfiguration(new CompetencyCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new CompetencyConfiguration());
        modelBuilder.ApplyConfiguration(new PositionCompetencyConfiguration());
        modelBuilder.ApplyConfiguration(new ReviewCycleConfiguration());
        modelBuilder.ApplyConfiguration(new AppraisalTemplateConfiguration());
        modelBuilder.ApplyConfiguration(new OrganizationalObjectiveConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeGoalConfiguration());
        modelBuilder.ApplyConfiguration(new GoalActionItemConfiguration());
        modelBuilder.ApplyConfiguration(new AppraisalConfiguration());
        modelBuilder.ApplyConfiguration(new AppraisalGoalConfiguration());
        modelBuilder.ApplyConfiguration(new AppraisalCompetencyConfiguration());
        modelBuilder.ApplyConfiguration(new AppraisalPeerReviewConfiguration());
        modelBuilder.ApplyConfiguration(new CalibrationSessionConfiguration());
        modelBuilder.ApplyConfiguration(new CalibrationItemConfiguration());
        modelBuilder.ApplyConfiguration(new PerformanceHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new IndividualDevelopmentPlanConfiguration());
        modelBuilder.ApplyConfiguration(new DevelopmentActionConfiguration());
        modelBuilder.ApplyConfiguration(new PerformanceImprovementPlanConfiguration());
        modelBuilder.ApplyConfiguration(new PipObjectiveConfiguration());
        modelBuilder.ApplyConfiguration(new AchievementConfiguration());
        modelBuilder.ApplyConfiguration(new RecognitionBadgeConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeRecognitionConfiguration());
        modelBuilder.ApplyConfiguration(new AwardCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new AllowanceTypeConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeAllowanceConfiguration());
        modelBuilder.ApplyConfiguration(new SalaryRevisionConfiguration());
        modelBuilder.ApplyConfiguration(new SalaryRevisionLineConfiguration());
        modelBuilder.ApplyConfiguration(new SalaryRevisionBandConfiguration());
        modelBuilder.ApplyConfiguration(new SalaryIncrementPolicyConfiguration());
        modelBuilder.ApplyConfiguration(new BenefitPlanConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeBenefitEnrollmentConfiguration());
        modelBuilder.ApplyConfiguration(new TaxBracketConfiguration());
        modelBuilder.ApplyConfiguration(new CompensationRequestConfiguration());
        modelBuilder.ApplyConfiguration(new ProfileChangeRequestConfiguration());
        modelBuilder.ApplyConfiguration(new MedicalProviderConfiguration());
        modelBuilder.ApplyConfiguration(new MedicalPlanConfiguration());
        modelBuilder.ApplyConfiguration(new MedicalServiceContractConfiguration());
        modelBuilder.ApplyConfiguration(new MedicalEnrollmentConfiguration());
        modelBuilder.ApplyConfiguration(new MedicalBeneficiaryConfiguration());
        modelBuilder.ApplyConfiguration(new MedicalClaimConfiguration());
        modelBuilder.ApplyConfiguration(new MedicalClaimAttachmentConfiguration());
        modelBuilder.ApplyConfiguration(new InsurancePolicyConfiguration());
        modelBuilder.ApplyConfiguration(new InsurancePremiumScheduleConfiguration());
        modelBuilder.ApplyConfiguration(new InsuranceClaimConfiguration());
        modelBuilder.ApplyConfiguration(new InsuranceClaimAttachmentConfiguration());
        modelBuilder.ApplyConfiguration(new LoanTypeConfiguration());
        modelBuilder.ApplyConfiguration(new LoanConfiguration());
        modelBuilder.ApplyConfiguration(new LoanGuarantorConfiguration());
        modelBuilder.ApplyConfiguration(new LoanRepaymentScheduleLineConfiguration());
        modelBuilder.ApplyConfiguration(new PerDiemRateConfiguration());
        modelBuilder.ApplyConfiguration(new TripBudgetConfiguration());
        modelBuilder.ApplyConfiguration(new TripRequestConfiguration());
        modelBuilder.ApplyConfiguration(new TripExpenseConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeGuaranteeConfiguration());
        modelBuilder.ApplyConfiguration(new OtherLeaveSettingConfiguration());
        modelBuilder.ApplyConfiguration(new OtherLeaveHeaderConfiguration());
        modelBuilder.ApplyConfiguration(new OtherLeaveDetailConfiguration());
        modelBuilder.ApplyConfiguration(new OtherLeaveAttachmentConfiguration());
        modelBuilder.ApplyConfiguration(new RecognitionProgramConfiguration());
        modelBuilder.ApplyConfiguration(new RewardNominationConfiguration());
        modelBuilder.ApplyConfiguration(new RewardPointsTransactionConfiguration());
        modelBuilder.ApplyConfiguration(new RewardDisbursementConfiguration());
        modelBuilder.ApplyConfiguration(new TrainingCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new TrainingCourseConfiguration());
        modelBuilder.ApplyConfiguration(new TrainingNeedConfiguration());
        modelBuilder.ApplyConfiguration(new TrainingSessionConfiguration());
        modelBuilder.ApplyConfiguration(new TrainingEnrollmentConfiguration());
        modelBuilder.ApplyConfiguration(new TrainingBudgetConfiguration());
        modelBuilder.ApplyConfiguration(new LearningPathConfiguration());
        modelBuilder.ApplyConfiguration(new LearningPathStepConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeTrainingCertificateConfiguration());
        modelBuilder.ApplyConfiguration(new TrainingProviderPaymentConfiguration());
        modelBuilder.ApplyConfiguration(new LearningCommunityConfiguration());
        modelBuilder.ApplyConfiguration(new LearningCommunityMemberConfiguration());
        modelBuilder.ApplyConfiguration(new LearningCommunityPostConfiguration());
        modelBuilder.ApplyConfiguration(new CommunityPostReactionConfiguration());
        modelBuilder.ApplyConfiguration(new SuggestionConfiguration());
        modelBuilder.ApplyConfiguration(new GrievanceConfiguration());
        modelBuilder.ApplyConfiguration(new GrievanceNoteConfiguration());
        modelBuilder.ApplyConfiguration(new AnnouncementConfiguration());
        modelBuilder.ApplyConfiguration(new CompanyAssetConfiguration());
        modelBuilder.ApplyConfiguration(new TerminationAssetRecoveryConfiguration());
        modelBuilder.ApplyConfiguration(new ExitQuestionnaireConfiguration());
        modelBuilder.ApplyConfiguration(new ExitInterviewConfiguration());
        modelBuilder.ApplyConfiguration(new TerminationSettlementConfiguration());
        modelBuilder.ApplyConfiguration(new SettlementLineConfiguration());
        modelBuilder.ApplyConfiguration(new SurveyConfiguration());
        modelBuilder.ApplyConfiguration(new SurveyResponseConfiguration());
        modelBuilder.ApplyConfiguration(new SurveyCompletionConfiguration());
        modelBuilder.ApplyConfiguration(new AppraisalAppealConfiguration());
        modelBuilder.ApplyConfiguration(new WorkforcePlanConfiguration());
        modelBuilder.ApplyConfiguration(new WorkforcePlanLineConfiguration());
        modelBuilder.ApplyConfiguration(new HiringRequestConfiguration());
        modelBuilder.ApplyConfiguration(new JobRequisitionConfiguration());
        modelBuilder.ApplyConfiguration(new RequisitionScreeningCriterionConfiguration());
        modelBuilder.ApplyConfiguration(new CriterionEvaluatorConfiguration());
        modelBuilder.ApplyConfiguration(new CandidateConfiguration());
        modelBuilder.ApplyConfiguration(new JobApplicationConfiguration());
        modelBuilder.ApplyConfiguration(new JobApplicationStageLogConfiguration());
        modelBuilder.ApplyConfiguration(new ApplicationCriterionScoreConfiguration());
        modelBuilder.ApplyConfiguration(new CandidateDocumentConfiguration());
        modelBuilder.ApplyConfiguration(new InterviewConfiguration());
        modelBuilder.ApplyConfiguration(new InterviewPanelistConfiguration());
        modelBuilder.ApplyConfiguration(new InterviewFeedbackConfiguration());
        modelBuilder.ApplyConfiguration(new JobOfferConfiguration());
        modelBuilder.ApplyConfiguration(new NumberSequenceConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
        modelBuilder.ApplyConfiguration(new ModuleConfiguration());
        modelBuilder.ApplyConfiguration(new OperationConfiguration());
        modelBuilder.ApplyConfiguration(new SubsystemConfiguration());
        modelBuilder.ApplyConfiguration(new CoreNotificationConfiguration());
        modelBuilder.ApplyConfiguration(new WorkflowDefinitionConfiguration());
        modelBuilder.ApplyConfiguration(new WorkflowStepConfiguration());
        modelBuilder.ApplyConfiguration(new WorkflowStepApproverConfiguration());
        modelBuilder.ApplyConfiguration(new WorkflowInstanceConfiguration());
        modelBuilder.ApplyConfiguration(new WorkflowActionLogConfiguration());
        modelBuilder.ApplyConfiguration(new DocumentTemplateConfiguration());
        modelBuilder.ApplyConfiguration(new OfferLetterTemplateConfiguration());
    }
}
