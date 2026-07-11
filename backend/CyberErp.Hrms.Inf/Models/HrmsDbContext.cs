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
                        optionsBuilder.UseSqlServer(conn, b => b.MigrationsAssembly("CyberErp.Hrms.Inf"));
                    }
                }
            }
        }
    }

    public DbSet<User> User { get; set; }
    public DbSet<Tenant> Tenant { get; set; }
    public DbSet<SubscriptionPlan> SubscriptionPlan { get; set; }
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
    public DbSet<LeaveBalance> LeaveBalance { get; set; }
    public DbSet<LeaveBalanceTransaction> LeaveBalanceTransaction { get; set; }
    public DbSet<JobCategory> JobCategory { get; set; }
    public DbSet<WorkLocation> WorkLocation { get; set; }

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
    public DbSet<RolePermission> RolePermission { get; set; }
    public DbSet<WorkflowDefinition> WorkflowDefinition { get; set; }
    public DbSet<WorkflowStep> WorkflowStep { get; set; }
    public DbSet<WorkflowStepApprover> WorkflowStepApprover { get; set; }
    public DbSet<WorkflowInstance> WorkflowInstance { get; set; }
    public DbSet<WorkflowActionLog> WorkflowActionLog { get; set; }
    public DbSet<DocumentTemplate> DocumentTemplate { get; set; }
    public DbSet<CompanyProfile> CompanyProfile { get; set; }

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
        modelBuilder.ApplyConfiguration(new LeaveBalanceConfiguration());
        modelBuilder.ApplyConfiguration(new LeaveBalanceTransactionConfiguration());
        modelBuilder.ApplyConfiguration(new JobCategoryConfiguration());
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
        modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
        modelBuilder.ApplyConfiguration(new WorkflowDefinitionConfiguration());
        modelBuilder.ApplyConfiguration(new WorkflowStepConfiguration());
        modelBuilder.ApplyConfiguration(new WorkflowStepApproverConfiguration());
        modelBuilder.ApplyConfiguration(new WorkflowInstanceConfiguration());
        modelBuilder.ApplyConfiguration(new WorkflowActionLogConfiguration());
        modelBuilder.ApplyConfiguration(new DocumentTemplateConfiguration());
        modelBuilder.ApplyConfiguration(new CompanyProfileConfiguration());
    }
}
