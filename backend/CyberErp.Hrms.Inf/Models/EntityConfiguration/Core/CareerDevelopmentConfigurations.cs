using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    // ===== Career Development §3.7.A — Succession Planning (HC148–HC160) =====

    public class CriticalPositionConfiguration : IEntityTypeConfiguration<CriticalPosition>
    {
        public void Configure(EntityTypeBuilder<CriticalPosition> builder)
        {
            builder.ToTable("hrmsCriticalPosition", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.RiskLevel).HasConversion<string>().HasMaxLength(20).IsRequired();
            // Default keeps pre-workflow rows (and direct-mode saves) operational without a backfill.
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired()
                .HasDefaultValue(CriticalPositionStatus.Active);
            builder.Property(x => x.Reason).HasMaxLength(1000);
            builder.Property(x => x.Criteria).HasMaxLength(2000);

            builder.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(x => x.Position).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(x => new { x.TenantId, x.PositionId }).IsUnique();
            builder.HasIndex(x => new { x.TenantId, x.IsActive });
        }
    }

    public class TalentReviewConfiguration : IEntityTypeConfiguration<TalentReview>
    {
        public void Configure(EntityTypeBuilder<TalentReview> builder)
        {
            builder.ToTable("hrmsTalentReview", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Cycle).HasMaxLength(60);
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.Notes).HasMaxLength(2000);

            builder.HasOne<OrganizationUnit>().WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.TenantId, x.Status });
        }
    }

    public class TalentAssessmentConfiguration : IEntityTypeConfiguration<TalentAssessment>
    {
        public void Configure(EntityTypeBuilder<TalentAssessment> builder)
        {
            builder.ToTable("hrmsTalentAssessment", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Readiness).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.Notes).HasMaxLength(2000);

            // Assessments belong to a review (cascade) and reference an employee (restrict).
            builder.HasOne<TalentReview>().WithMany().HasForeignKey(x => x.TalentReviewId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(x => x.Employee).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(x => x.Ratings).WithOne().HasForeignKey(r => r.TalentAssessmentId).OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(x => x.Ratings).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(x => new { x.TalentReviewId, x.EmployeeId }).IsUnique();
            // 9-box / heat-map / distribution counts are a single GROUP BY over this covering index.
            builder.HasIndex(x => new { x.TenantId, x.TalentReviewId, x.PerformanceBand, x.PotentialBand });
        }
    }

    public class TalentRatingConfiguration : IEntityTypeConfiguration<TalentRating>
    {
        public void Configure(EntityTypeBuilder<TalentRating> builder)
        {
            builder.ToTable("hrmsTalentRating", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.RaterRole).HasMaxLength(100);
            builder.Property(x => x.PerformanceScore).HasPrecision(6, 2);
            builder.Property(x => x.PotentialScore).HasPrecision(6, 2);
            builder.Property(x => x.Comment).HasMaxLength(1000);

            builder.HasOne<Employee>().WithMany().HasForeignKey(x => x.RaterEmployeeId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.TalentAssessmentId);
        }
    }

    public class SuccessionPlanConfiguration : IEntityTypeConfiguration<SuccessionPlan>
    {
        public void Configure(EntityTypeBuilder<SuccessionPlan> builder)
        {
            builder.ToTable("hrmsSuccessionPlan", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Horizon).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.Notes).HasMaxLength(2000);

            builder.HasOne(x => x.CriticalPosition).WithMany().HasForeignKey(x => x.CriticalPositionId).OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(x => x.CriticalPosition).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(x => new { x.TenantId, x.CriticalPositionId });
            builder.HasIndex(x => new { x.TenantId, x.Status });
        }
    }

    public class SuccessionCandidateConfiguration : IEntityTypeConfiguration<SuccessionCandidate>
    {
        public void Configure(EntityTypeBuilder<SuccessionCandidate> builder)
        {
            builder.ToTable("hrmsSuccessionCandidate", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Readiness).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.ReadinessScore).HasPrecision(5, 2);
            builder.Property(x => x.GapSummary).HasMaxLength(2000);
            builder.Property(x => x.Notes).HasMaxLength(2000);

            builder.HasOne<SuccessionPlan>().WithMany().HasForeignKey(x => x.SuccessionPlanId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(x => x.Employee).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(x => x.DevelopmentActions).WithOne().HasForeignKey(a => a.SuccessionCandidateId).OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(x => x.DevelopmentActions).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasMany(x => x.KnowledgeTransfers).WithOne().HasForeignKey(k => k.SuccessionCandidateId).OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(x => x.KnowledgeTransfers).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(x => new { x.SuccessionPlanId, x.Rank });
            builder.HasIndex(x => new { x.SuccessionPlanId, x.EmployeeId }).IsUnique();
            builder.HasIndex(x => new { x.TenantId, x.EmployeeId });
        }
    }

    public class SuccessionDevelopmentActionConfiguration : IEntityTypeConfiguration<SuccessionDevelopmentAction>
    {
        public void Configure(EntityTypeBuilder<SuccessionDevelopmentAction> builder)
        {
            builder.ToTable("hrmsSuccessionDevelopmentAction", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.Description).IsRequired().HasMaxLength(2000);

            builder.HasOne<Employee>().WithMany().HasForeignKey(x => x.MentorEmployeeId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.SuccessionCandidateId);
        }
    }

    public class KnowledgeTransferConfiguration : IEntityTypeConfiguration<KnowledgeTransfer>
    {
        public void Configure(EntityTypeBuilder<KnowledgeTransfer> builder)
        {
            builder.ToTable("hrmsKnowledgeTransfer", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Topic).IsRequired().HasMaxLength(300);
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

            builder.HasOne<Employee>().WithMany().HasForeignKey(x => x.FromEmployeeId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.SuccessionCandidateId);
        }
    }
}
