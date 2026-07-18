using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    // Performance Management (HC118–HC147) — Phase A configuration tables.

    public class RatingScaleConfiguration : IEntityTypeConfiguration<RatingScale>
    {
        public void Configure(EntityTypeBuilder<RatingScale> builder)
        {
            builder.ToTable("hrmsRatingScale", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Description).HasMaxLength(1000);
            builder.Property(x => x.ScoreType).HasConversion<string>().HasMaxLength(20).IsRequired();

            builder.HasMany(x => x.Levels)
                .WithOne()
                .HasForeignKey(l => l.RatingScaleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
        }
    }

    public class RatingScaleLevelConfiguration : IEntityTypeConfiguration<RatingScaleLevel>
    {
        public void Configure(EntityTypeBuilder<RatingScaleLevel> builder)
        {
            builder.ToTable("hrmsRatingScaleLevel", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Label).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.Property(x => x.MinScore).HasPrecision(6, 2);
            builder.Property(x => x.MaxScore).HasPrecision(6, 2);

            builder.HasIndex(x => new { x.RatingScaleId, x.Value }).IsUnique();
        }
    }

    public class CompetencyCategoryConfiguration : IEntityTypeConfiguration<CompetencyCategory>
    {
        public void Configure(EntityTypeBuilder<CompetencyCategory> builder)
        {
            builder.ToTable("hrmsCompetencyCategory", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
            builder.Property(x => x.Description).HasMaxLength(1000);

            builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
        }
    }

    public class CompetencyConfiguration : IEntityTypeConfiguration<Competency>
    {
        public void Configure(EntityTypeBuilder<Competency> builder)
        {
            builder.ToTable("hrmsCompetency", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Description).HasMaxLength(2000);

            builder.HasOne<CompetencyCategory>()
                .WithMany()
                .HasForeignKey(x => x.CompetencyCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
            builder.HasIndex(x => x.CompetencyCategoryId);
        }
    }

    public class PositionCompetencyConfiguration : IEntityTypeConfiguration<PositionCompetency>
    {
        public void Configure(EntityTypeBuilder<PositionCompetency> builder)
        {
            builder.ToTable("hrmsPositionCompetency", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Weight).HasPrecision(5, 2);

            // A mapping is meaningless without its position → cascade; the competency is Restrict
            // (a competency in use cannot be deleted). Only one cascade path, so no multiple-path issue.
            builder.HasOne<Position>()
                .WithMany()
                .HasForeignKey(x => x.PositionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<Competency>()
                .WithMany()
                .HasForeignKey(x => x.CompetencyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.PositionId, x.CompetencyId }).IsUnique();
        }
    }

    public class ReviewCycleConfiguration : IEntityTypeConfiguration<ReviewCycle>
    {
        public void Configure(EntityTypeBuilder<ReviewCycle> builder)
        {
            builder.ToTable("hrmsReviewCycle", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.PeriodType).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

            builder.HasOne<RatingScale>()
                .WithMany()
                .HasForeignKey(x => x.RatingScaleId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<FiscalYear>()
                .WithMany()
                .HasForeignKey(x => x.FiscalYearId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
            builder.HasIndex(x => new { x.TenantId, x.Status });
        }
    }

    public class AppraisalTemplateConfiguration : IEntityTypeConfiguration<AppraisalTemplate>
    {
        public void Configure(EntityTypeBuilder<AppraisalTemplate> builder)
        {
            builder.ToTable("hrmsAppraisalTemplate", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Description).HasMaxLength(2000);
            builder.Property(x => x.GoalsWeight).HasPrecision(5, 2);
            builder.Property(x => x.CompetenciesWeight).HasPrecision(5, 2);

            builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
        }
    }

    // Performance Management — Phase B: objectives, goals & action plans (HC118–HC122).

    public class OrganizationalObjectiveConfiguration : IEntityTypeConfiguration<OrganizationalObjective>
    {
        public void Configure(EntityTypeBuilder<OrganizationalObjective> builder)
        {
            builder.ToTable("hrmsOrganizationalObjective", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title).IsRequired().HasMaxLength(300);
            builder.Property(x => x.Description).HasMaxLength(2000);
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.Weight).HasPrecision(5, 2);

            // All lookups are Restrict — an objective in use blocks the delete (guarded in the handler too).
            builder.HasOne<ReviewCycle>()
                .WithMany()
                .HasForeignKey(x => x.ReviewCycleId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<OrganizationUnit>()
                .WithMany()
                .HasForeignKey(x => x.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);
            // Self-reference for the cascade hierarchy (organization → directorate → team).
            builder.HasOne<OrganizationalObjective>()
                .WithMany()
                .HasForeignKey(x => x.ParentObjectiveId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.TenantId, x.ReviewCycleId });
            builder.HasIndex(x => x.ParentObjectiveId);
            builder.HasIndex(x => new { x.TenantId, x.ReviewCycleId, x.Title }).IsUnique();
        }
    }

    public class EmployeeGoalConfiguration : IEntityTypeConfiguration<EmployeeGoal>
    {
        public void Configure(EntityTypeBuilder<EmployeeGoal> builder)
        {
            builder.ToTable("hrmsEmployeeGoal", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title).IsRequired().HasMaxLength(300);
            builder.Property(x => x.Description).HasMaxLength(2000);
            builder.Property(x => x.Measure).HasMaxLength(500);
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.Weight).HasPrecision(5, 2);
            builder.Property(x => x.TargetValue).HasPrecision(18, 2);

            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<ReviewCycle>()
                .WithMany()
                .HasForeignKey(x => x.ReviewCycleId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<OrganizationalObjective>()
                .WithMany()
                .HasForeignKey(x => x.OrganizationalObjectiveId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.ActionItems)
                .WithOne()
                .HasForeignKey(a => a.EmployeeGoalId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.TenantId, x.EmployeeId, x.ReviewCycleId });
            builder.HasIndex(x => x.ReviewCycleId);
            builder.HasIndex(x => x.OrganizationalObjectiveId);
        }
    }

    public class GoalActionItemConfiguration : IEntityTypeConfiguration<GoalActionItem>
    {
        public void Configure(EntityTypeBuilder<GoalActionItem> builder)
        {
            builder.ToTable("hrmsGoalActionItem", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Description).IsRequired().HasMaxLength(500);

            builder.HasIndex(x => new { x.EmployeeGoalId, x.SortOrder });
        }
    }

    // Performance Management — Phase C1: scored appraisals (HC127/HC133/HC138).

    public class AppraisalConfiguration : IEntityTypeConfiguration<Appraisal>
    {
        public void Configure(EntityTypeBuilder<Appraisal> builder)
        {
            builder.ToTable("hrmsAppraisal", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Stage).HasConversion<string>().HasMaxLength(30).IsRequired();
            builder.Property(x => x.GoalsWeight).HasPrecision(5, 2);
            builder.Property(x => x.CompetenciesWeight).HasPrecision(5, 2);
            builder.Property(x => x.OverallScore).HasPrecision(6, 2);
            builder.Property(x => x.SelfComments).HasMaxLength(4000);
            builder.Property(x => x.ManagerComments).HasMaxLength(4000);
            builder.Property(x => x.AcknowledgmentStatus).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.EmployeeSignature).HasMaxLength(200);
            builder.Property(x => x.ManagerSignature).HasMaxLength(200);
            builder.Property(x => x.ReviewerComments).HasMaxLength(4000);
            builder.Property(x => x.ReviewerSignature).HasMaxLength(200);
            builder.Property(x => x.HrSignature).HasMaxLength(200);

            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<ReviewCycle>()
                .WithMany()
                .HasForeignKey(x => x.ReviewCycleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Goals)
                .WithOne()
                .HasForeignKey(g => g.AppraisalId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(x => x.Competencies)
                .WithOne()
                .HasForeignKey(c => c.AppraisalId)
                .OnDelete(DeleteBehavior.Cascade);

            // One appraisal per employee per cycle.
            builder.HasIndex(x => new { x.TenantId, x.EmployeeId, x.ReviewCycleId }).IsUnique();
            builder.HasIndex(x => new { x.TenantId, x.ReviewCycleId, x.Stage });
        }
    }

    public class AppraisalGoalConfiguration : IEntityTypeConfiguration<AppraisalGoal>
    {
        public void Configure(EntityTypeBuilder<AppraisalGoal> builder)
        {
            builder.ToTable("hrmsAppraisalGoal", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title).IsRequired().HasMaxLength(300);
            builder.Property(x => x.Weight).HasPrecision(5, 2);
            builder.Property(x => x.SelfScore).HasPrecision(6, 2);
            builder.Property(x => x.ManagerScore).HasPrecision(6, 2);
            builder.Property(x => x.SelfComments).HasMaxLength(2000);
            builder.Property(x => x.ManagerComments).HasMaxLength(2000);

            builder.HasIndex(x => new { x.AppraisalId, x.SortOrder });
        }
    }

    public class AppraisalCompetencyConfiguration : IEntityTypeConfiguration<AppraisalCompetency>
    {
        public void Configure(EntityTypeBuilder<AppraisalCompetency> builder)
        {
            builder.ToTable("hrmsAppraisalCompetency", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CompetencyName).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Weight).HasPrecision(5, 2);
            builder.Property(x => x.SelfScore).HasPrecision(6, 2);
            builder.Property(x => x.ManagerScore).HasPrecision(6, 2);
            builder.Property(x => x.SelfComments).HasMaxLength(2000);
            builder.Property(x => x.ManagerComments).HasMaxLength(2000);

            builder.HasIndex(x => new { x.AppraisalId, x.SortOrder });
        }
    }

    // Performance Management — Phase C2: peer review, calibration, version history (HC127/128/129/132).

    public class AppraisalPeerReviewConfiguration : IEntityTypeConfiguration<AppraisalPeerReview>
    {
        public void Configure(EntityTypeBuilder<AppraisalPeerReview> builder)
        {
            builder.ToTable("hrmsAppraisalPeerReview", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.Score).HasPrecision(6, 2);
            builder.Property(x => x.Comments).HasMaxLength(2000);

            builder.HasOne<Appraisal>()
                .WithMany()
                .HasForeignKey(x => x.AppraisalId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(x => x.PeerEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.AppraisalId, x.PeerEmployeeId }).IsUnique();
        }
    }

    public class CalibrationSessionConfiguration : IEntityTypeConfiguration<CalibrationSession>
    {
        public void Configure(EntityTypeBuilder<CalibrationSession> builder)
        {
            builder.ToTable("hrmsCalibrationSession", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.Notes).HasMaxLength(2000);

            builder.HasOne<ReviewCycle>()
                .WithMany()
                .HasForeignKey(x => x.ReviewCycleId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<OrganizationUnit>()
                .WithMany()
                .HasForeignKey(x => x.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Items)
                .WithOne()
                .HasForeignKey(i => i.CalibrationSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.TenantId, x.ReviewCycleId });
        }
    }

    public class CalibrationItemConfiguration : IEntityTypeConfiguration<CalibrationItem>
    {
        public void Configure(EntityTypeBuilder<CalibrationItem> builder)
        {
            builder.ToTable("hrmsCalibrationItem", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.OriginalScore).HasPrecision(6, 2);
            builder.Property(x => x.CalibratedScore).HasPrecision(6, 2);
            builder.Property(x => x.Justification).HasMaxLength(2000);

            // Appraisal is Restrict (a calibration item must not cascade-delete an appraisal).
            builder.HasOne<Appraisal>()
                .WithMany()
                .HasForeignKey(x => x.AppraisalId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.CalibrationSessionId);
            builder.HasIndex(x => x.AppraisalId);
        }
    }

    public class PerformanceHistoryConfiguration : IEntityTypeConfiguration<PerformanceHistory>
    {
        public void Configure(EntityTypeBuilder<PerformanceHistory> builder)
        {
            builder.ToTable("hrmsPerformanceHistory", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.EntityType).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Action).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Summary).HasMaxLength(1000);
            builder.Property(x => x.SnapshotJson);   // nvarchar(max)

            // The read path is always "timeline for one entity" — cover it.
            builder.HasIndex(x => new { x.TenantId, x.EntityType, x.EntityId });
        }
    }

    // Performance Management — Phase D1: development plans (IDP) & improvement plans (PIP) (HC130/131/135).

    public class IndividualDevelopmentPlanConfiguration : IEntityTypeConfiguration<IndividualDevelopmentPlan>
    {
        public void Configure(EntityTypeBuilder<IndividualDevelopmentPlan> builder)
        {
            builder.ToTable("hrmsDevelopmentPlan", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title).IsRequired().HasMaxLength(300);
            builder.Property(x => x.Description).HasMaxLength(2000);
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Appraisal>()
                .WithMany()
                .HasForeignKey(x => x.AppraisalId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Actions)
                .WithOne()
                .HasForeignKey(a => a.DevelopmentPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.TenantId, x.EmployeeId });
        }
    }

    public class DevelopmentActionConfiguration : IEntityTypeConfiguration<DevelopmentAction>
    {
        public void Configure(EntityTypeBuilder<DevelopmentAction> builder)
        {
            builder.ToTable("hrmsDevelopmentAction", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Description).IsRequired().HasMaxLength(500);
            builder.Property(x => x.LearningIntervention).HasMaxLength(200);
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

            // Competency is a soft reference (the gap addressed) — Restrict, nullable.
            builder.HasOne<Competency>()
                .WithMany()
                .HasForeignKey(x => x.CompetencyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.DevelopmentPlanId, x.SortOrder });
        }
    }

    public class PerformanceImprovementPlanConfiguration : IEntityTypeConfiguration<PerformanceImprovementPlan>
    {
        public void Configure(EntityTypeBuilder<PerformanceImprovementPlan> builder)
        {
            builder.ToTable("hrmsImprovementPlan", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title).IsRequired().HasMaxLength(300);
            builder.Property(x => x.Reason).IsRequired().HasMaxLength(2000);
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.Outcome).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.OutcomeNotes).HasMaxLength(2000);

            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Appraisal>()
                .WithMany()
                .HasForeignKey(x => x.AppraisalId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Objectives)
                .WithOne()
                .HasForeignKey(o => o.PipId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.TenantId, x.EmployeeId });
        }
    }

    public class PipObjectiveConfiguration : IEntityTypeConfiguration<PipObjective>
    {
        public void Configure(EntityTypeBuilder<PipObjective> builder)
        {
            builder.ToTable("hrmsPipObjective", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Description).IsRequired().HasMaxLength(500);
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

            builder.HasIndex(x => new { x.PipId, x.SortOrder });
        }
    }

    // Performance Management — Phase D2: achievements & recognition (HC139–141).

    public class AchievementConfiguration : IEntityTypeConfiguration<Achievement>
    {
        public void Configure(EntityTypeBuilder<Achievement> builder)
        {
            builder.ToTable("hrmsAchievement", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title).IsRequired().HasMaxLength(300);
            builder.Property(x => x.Description).HasMaxLength(2000);
            builder.Property(x => x.Category).HasConversion<string>().HasMaxLength(20).IsRequired();

            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Appraisal>()
                .WithMany()
                .HasForeignKey(x => x.AppraisalId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.TenantId, x.EmployeeId });
        }
    }

    public class RecognitionBadgeConfiguration : IEntityTypeConfiguration<RecognitionBadge>
    {
        public void Configure(EntityTypeBuilder<RecognitionBadge> builder)
        {
            builder.ToTable("hrmsRecognitionBadge", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
            builder.Property(x => x.Description).HasMaxLength(1000);
            builder.Property(x => x.Color).HasMaxLength(20);
            builder.Property(x => x.Icon).HasMaxLength(50);

            builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
        }
    }

    public class AppraisalAppealConfiguration : IEntityTypeConfiguration<AppraisalAppeal>
    {
        public void Configure(EntityTypeBuilder<AppraisalAppeal> builder)
        {
            builder.ToTable("hrmsAppraisalAppeal", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Comments).IsRequired().HasMaxLength(2000);
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.Resolution).HasMaxLength(2000);

            builder.HasOne<Appraisal>()
                .WithMany()
                .HasForeignKey(x => x.AppraisalId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.AppraisalId);
            builder.HasIndex(x => new { x.TenantId, x.Status });
        }
    }

    public class EmployeeRecognitionConfiguration : IEntityTypeConfiguration<EmployeeRecognition>
    {
        public void Configure(EntityTypeBuilder<EmployeeRecognition> builder)
        {
            builder.ToTable("hrmsEmployeeRecognition", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Citation).IsRequired().HasMaxLength(1000);

            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<RecognitionBadge>()
                .WithMany()
                .HasForeignKey(x => x.RecognitionBadgeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.TenantId, x.EmployeeId });
            builder.HasIndex(x => new { x.TenantId, x.IsPublic });
        }
    }
}
