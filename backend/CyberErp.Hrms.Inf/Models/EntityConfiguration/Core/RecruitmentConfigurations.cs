using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class HiringRequestConfiguration : IEntityTypeConfiguration<HiringRequest>
    {
        public void Configure(EntityTypeBuilder<HiringRequest> builder)
        {
            builder.ToTable("hrms_HiringRequest", "Core");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.RequestNumber).IsRequired().HasMaxLength(30);
            builder.Property(r => r.EmploymentType).IsRequired().HasConversion<string>().HasMaxLength(30);
            builder.Property(r => r.Status).IsRequired().HasConversion<string>().HasMaxLength(30);
            builder.Property(r => r.Justification).IsRequired().HasMaxLength(2000);
            builder.Property(r => r.JobRequirements).HasMaxLength(2000);
            builder.Property(r => r.TimelineRemarks).HasMaxLength(1000);
            builder.Property(r => r.EstimatedBudget).HasColumnType("decimal(18,2)");
            // WorkforcePlanId is a link snapshot (no FK) — plans version and archive independently.

            builder.HasOne<OrganizationUnit>()
                .WithMany()
                .HasForeignKey(r => r.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<PositionClass>()
                .WithMany()
                .HasForeignKey(r => r.PositionClassId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(r => new { r.TenantId, r.RequestNumber }).IsUnique();
            builder.HasIndex(r => new { r.TenantId, r.Status });
            builder.HasIndex(r => r.OrganizationUnitId);
        }
    }

    public class JobRequisitionConfiguration : IEntityTypeConfiguration<JobRequisition>
    {
        public void Configure(EntityTypeBuilder<JobRequisition> builder)
        {
            builder.ToTable("hrms_JobRequisition", "Core");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.RequisitionNumber).IsRequired().HasMaxLength(30);
            builder.Property(r => r.EmploymentType).IsRequired().HasConversion<string>().HasMaxLength(30);
            builder.Property(r => r.PostingChannel).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(r => r.Status).IsRequired().HasConversion<string>().HasMaxLength(30);
            builder.Property(r => r.Title).IsRequired().HasMaxLength(200);
            builder.Property(r => r.Description).HasMaxLength(4000);
            builder.Property(r => r.MinQualifications).HasMaxLength(1000);
            builder.Property(r => r.Skills).HasMaxLength(1000);
            builder.Property(r => r.PostingText).HasMaxLength(8000);

            builder.HasOne<HiringRequest>()
                .WithMany()
                .HasForeignKey(r => r.HiringRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<OrganizationUnit>()
                .WithMany()
                .HasForeignKey(r => r.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<PositionClass>()
                .WithMany()
                .HasForeignKey(r => r.PositionClassId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<WorkLocation>()
                .WithMany()
                .HasForeignKey(r => r.WorkLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<SalaryScale>()
                .WithMany()
                .HasForeignKey(r => r.SalaryScaleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(r => r.ScreeningCriteria)
                .WithOne()
                .HasForeignKey(c => c.RequisitionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(r => r.ScreeningCriteria).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(r => new { r.TenantId, r.RequisitionNumber }).IsUnique();
            builder.HasIndex(r => new { r.TenantId, r.Status });
            builder.HasIndex(r => r.HiringRequestId);
            builder.HasIndex(r => r.OrganizationUnitId);
        }
    }

    public class RequisitionScreeningCriterionConfiguration : IEntityTypeConfiguration<RequisitionScreeningCriterion>
    {
        public void Configure(EntityTypeBuilder<RequisitionScreeningCriterion> builder)
        {
            builder.ToTable("hrms_RequisitionScreeningCriterion", "Core");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name).IsRequired().HasMaxLength(300);
            builder.Property(c => c.AppliesAtStage).HasConversion<string>().HasMaxLength(30);

            builder.HasMany(c => c.Evaluators)
                .WithOne()
                .HasForeignKey(e => e.CriterionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(c => c.Evaluators).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(c => c.RequisitionId);
        }
    }

    public class CriterionEvaluatorConfiguration : IEntityTypeConfiguration<CriterionEvaluator>
    {
        public void Configure(EntityTypeBuilder<CriterionEvaluator> builder)
        {
            builder.ToTable("hrms_CriterionEvaluator", "Core");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.EvaluatorType).IsRequired().HasConversion<string>().HasMaxLength(30);
            builder.Property(e => e.Name).IsRequired().HasMaxLength(300);

            // The assignment must not block employee deletion (the name snapshot survives).
            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(e => e.CriterionId);
        }
    }

    public class ApplicationCriterionScoreConfiguration : IEntityTypeConfiguration<ApplicationCriterionScore>
    {
        public void Configure(EntityTypeBuilder<ApplicationCriterionScore> builder)
        {
            builder.ToTable("hrms_ApplicationCriterionScore", "Core");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Score).HasColumnType("decimal(5,2)");
            builder.Property(s => s.Remarks).HasMaxLength(1000);
            builder.Property(s => s.ScoredBy).HasMaxLength(200);

            // Criterion reference kept loose (criteria are replaced wholesale on requisition edits;
            // the score row keeps the weight snapshot, so a dangling id only loses the name).
            builder.HasIndex(s => new { s.ApplicationId, s.CriterionId }).IsUnique();
        }
    }

    public class CandidateDocumentConfiguration : IEntityTypeConfiguration<CandidateDocument>
    {
        public void Configure(EntityTypeBuilder<CandidateDocument> builder)
        {
            builder.ToTable("hrms_CandidateDocument", "Core");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.DocumentType).IsRequired().HasConversion<string>().HasMaxLength(40);
            builder.Property(d => d.FileName).IsRequired().HasMaxLength(300);
            builder.Property(d => d.ContentType).IsRequired().HasMaxLength(100);

            builder.HasOne<Candidate>()
                .WithMany()
                .HasForeignKey(d => d.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(d => new { d.CandidateId, d.DocumentType });
        }
    }

    public class CandidateConfiguration : IEntityTypeConfiguration<Candidate>
    {
        public void Configure(EntityTypeBuilder<Candidate> builder)
        {
            builder.ToTable("hrms_Candidate", "Core");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.CandidateNumber).IsRequired().HasMaxLength(30);
            builder.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
            builder.Property(c => c.FatherName).HasMaxLength(100);
            builder.Property(c => c.GrandFatherName).HasMaxLength(100);
            builder.Property(c => c.Email).HasMaxLength(200);
            builder.Property(c => c.PhoneNumber).HasMaxLength(50);
            builder.Property(c => c.Gender).HasConversion<string>().HasMaxLength(20);
            builder.Property(c => c.Source).IsRequired().HasConversion<string>().HasMaxLength(30);
            builder.Property(c => c.EducationSummary).HasMaxLength(2000);
            builder.Property(c => c.ExperienceSummary).HasMaxLength(2000);
            builder.Property(c => c.SkillsSummary).HasMaxLength(1000);
            builder.Property(c => c.ResumeFileName).HasMaxLength(300);
            builder.Property(c => c.TalentPoolNotes).HasMaxLength(1000);
            builder.Ignore(c => c.FullName);

            // An internal candidate's employee link must not block employee deletion.
            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(c => c.InternalEmployeeId)
                .OnDelete(DeleteBehavior.SetNull);

            // The shared person record backing the candidate (hire-conversion anchor).
            builder.HasOne<Person>()
                .WithMany()
                .HasForeignKey(c => c.PersonId)
                .OnDelete(DeleteBehavior.Restrict);

            // HiredEmployeeId is a historical pointer (no FK — a second SET NULL path from Employee
            // would create multiple cascade paths on SQL Server; InternalEmployeeId already has one).
            builder.HasIndex(c => c.HiredEmployeeId);

            builder.HasIndex(c => new { c.TenantId, c.CandidateNumber }).IsUnique();
            builder.HasIndex(c => new { c.TenantId, c.IsInTalentPool });
            builder.HasIndex(c => c.Email);
        }
    }

    public class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
    {
        public void Configure(EntityTypeBuilder<JobApplication> builder)
        {
            builder.ToTable("hrms_JobApplication", "Core");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Stage).IsRequired().HasConversion<string>().HasMaxLength(30);
            builder.Property(a => a.ScreeningScore).HasColumnType("decimal(5,2)");
            builder.Property(a => a.ScreeningRemarks).HasMaxLength(2000);

            builder.HasOne<Candidate>()
                .WithMany()
                .HasForeignKey(a => a.CandidateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<JobRequisition>()
                .WithMany()
                .HasForeignKey(a => a.RequisitionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(a => a.StageLog)
                .WithOne()
                .HasForeignKey(l => l.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(a => a.StageLog).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(a => a.CriterionScores)
                .WithOne()
                .HasForeignKey(s => s.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(a => a.CriterionScores).UsePropertyAccessMode(PropertyAccessMode.Field);

            // One application per candidate per requisition (HC096/HC098).
            builder.HasIndex(a => new { a.CandidateId, a.RequisitionId }).IsUnique();
            builder.HasIndex(a => new { a.TenantId, a.Stage });
            builder.HasIndex(a => a.RequisitionId);
        }
    }

    public class JobApplicationStageLogConfiguration : IEntityTypeConfiguration<JobApplicationStageLog>
    {
        public void Configure(EntityTypeBuilder<JobApplicationStageLog> builder)
        {
            builder.ToTable("hrms_JobApplicationStageLog", "Core");

            builder.HasKey(l => l.Id);

            builder.Property(l => l.Stage).IsRequired().HasConversion<string>().HasMaxLength(30);
            builder.Property(l => l.Note).HasMaxLength(1000);
            builder.Property(l => l.ActedBy).HasMaxLength(200);

            builder.HasIndex(l => l.ApplicationId);
        }
    }
}
