using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    // Training & Development — §3.8 (HC187–HC202): catalog + training needs (Phase TD1).

    public class TrainingCategoryConfiguration : IEntityTypeConfiguration<TrainingCategory>
    {
        public void Configure(EntityTypeBuilder<TrainingCategory> builder)
        {
            builder.ToTable("TrainingCategory", "Hrms");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
            builder.Property(x => x.Description).HasMaxLength(1000);

            builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
        }
    }

    public class CourseVersionConfiguration : IEntityTypeConfiguration<CourseVersion>
    {
        public void Configure(EntityTypeBuilder<CourseVersion> builder)
        {
            builder.ToTable("CourseVersion", "Hrms");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.ChangeNote).HasMaxLength(1000);

            builder.HasOne<TrainingCourse>()
                .WithMany()
                .HasForeignKey(x => x.TrainingCourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Modules are owned by their version: they have no meaning apart from it, and a version
            // is immutable once published, so they are never re-parented.
            builder.HasMany(x => x.Modules)
                .WithOne()
                .HasForeignKey(m => m.CourseVersionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(x => x.Modules).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(x => new { x.TrainingCourseId, x.VersionNumber }).IsUnique();
        }
    }

    public class ContentModuleConfiguration : IEntityTypeConfiguration<ContentModule>
    {
        public void Configure(EntityTypeBuilder<ContentModule> builder)
        {
            builder.ToTable("ContentModule", "Hrms");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title).IsRequired().HasMaxLength(300);
            builder.Property(x => x.Kind).HasConversion<string>().HasMaxLength(20).IsRequired();
            // Authored rich text — the same ceiling the other long-form content fields use.
            builder.Property(x => x.Body).HasMaxLength(4000);
            builder.Property(x => x.ExternalUrl).HasMaxLength(1000);

            builder.HasIndex(x => new { x.CourseVersionId, x.SortOrder });
        }
    }

    public class ModuleProgressConfiguration : IEntityTypeConfiguration<ModuleProgress>
    {
        public void Configure(EntityTypeBuilder<ModuleProgress> builder)
        {
            builder.ToTable("ModuleProgress", "Hrms");
            builder.HasKey(x => x.Id);

            // Progress dies with the enrolment it belongs to. The module side is Restrict: only one
            // cascade path may reach this table, and a published version's modules are never deleted
            // anyway.
            builder.HasOne<TrainingEnrollment>()
                .WithMany()
                .HasForeignKey(x => x.TrainingEnrollmentId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<ContentModule>()
                .WithMany()
                .HasForeignKey(x => x.ContentModuleId)
                .OnDelete(DeleteBehavior.Restrict);

            // One row per (enrolment, module): a second visit updates the row rather than adding one.
            builder.HasIndex(x => new { x.TrainingEnrollmentId, x.ContentModuleId }).IsUnique();
        }
    }

    // Assessment — §3.8 phase 4 (logic §12.87). Auto-graded quizzes inside a course version.

    public class QuestionBankConfiguration : IEntityTypeConfiguration<QuestionBank>
    {
        public void Configure(EntityTypeBuilder<QuestionBank> builder)
        {
            builder.ToTable("QuestionBank", "Hrms");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Description).HasMaxLength(1000);

            builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
        }
    }

    public class AssessmentConfiguration : IEntityTypeConfiguration<Assessment>
    {
        public void Configure(EntityTypeBuilder<Assessment> builder)
        {
            builder.ToTable("Assessment", "Hrms");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title).IsRequired().HasMaxLength(300);
            builder.Property(x => x.Instructions).HasMaxLength(2000);
            builder.Property(x => x.PassMark).HasPrecision(5, 2);

            // The quiz dies with the module it is the content of, which in turn dies with its
            // version. One owner, one cascade path — and the assessment is never re-parented,
            // because a quiz that moved between courses would silently invalidate every attempt
            // already recorded against it.
            builder.HasOne<ContentModule>()
                .WithMany()
                .HasForeignKey(x => x.ContentModuleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Questions)
                .WithOne()
                .HasForeignKey(q => q.AssessmentId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(x => x.Questions).UsePropertyAccessMode(PropertyAccessMode.Field);

            // One assessment per module: a Quiz module IS its assessment.
            builder.HasIndex(x => x.ContentModuleId).IsUnique();
        }
    }

    public class QuestionConfiguration : IEntityTypeConfiguration<Question>
    {
        public void Configure(EntityTypeBuilder<Question> builder)
        {
            builder.ToTable("Question", "Hrms");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Text).IsRequired().HasMaxLength(2000);
            builder.Property(x => x.Kind).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.Points).HasPrecision(6, 2);
            builder.Property(x => x.Explanation).HasMaxLength(2000);

            // A question belongs to EITHER a bank or an assessment. Both FKs are optional and both
            // cascade; they are unrelated roots, so there is no shared ancestor and no multiple
            // cascade path. The assessment side is declared on Assessment.Questions.
            builder.HasOne<QuestionBank>()
                .WithMany()
                .HasForeignKey(x => x.QuestionBankId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Options)
                .WithOne()
                .HasForeignKey(o => o.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(x => x.Options).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(x => new { x.QuestionBankId, x.SortOrder });
            builder.HasIndex(x => new { x.AssessmentId, x.SortOrder });
        }
    }

    public class QuestionOptionConfiguration : IEntityTypeConfiguration<QuestionOption>
    {
        public void Configure(EntityTypeBuilder<QuestionOption> builder)
        {
            builder.ToTable("QuestionOption", "Hrms");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Text).IsRequired().HasMaxLength(1000);

            builder.HasIndex(x => new { x.QuestionId, x.SortOrder });
        }
    }

    public class AssessmentAttemptConfiguration : IEntityTypeConfiguration<AssessmentAttempt>
    {
        public void Configure(EntityTypeBuilder<AssessmentAttempt> builder)
        {
            builder.ToTable("AssessmentAttempt", "Hrms");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.PointsAwarded).HasPrecision(9, 2);
            builder.Property(x => x.PointsPossible).HasPrecision(9, 2);
            builder.Property(x => x.ScorePercent).HasPrecision(5, 2);

            // Attempts die with the enrolment, as ModuleProgress does, but are RESTRICTED against
            // the assessment: an attempt is the evidence behind a pass, and a published assessment
            // is never deleted anyway.
            builder.HasOne<TrainingEnrollment>()
                .WithMany()
                .HasForeignKey(x => x.TrainingEnrollmentId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<Assessment>()
                .WithMany()
                .HasForeignKey(x => x.AssessmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Answers)
                .WithOne()
                .HasForeignKey(a => a.AssessmentAttemptId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(x => x.Answers).UsePropertyAccessMode(PropertyAccessMode.Field);

            // One attempt number per (enrolment, assessment): the retake counter cannot collide even
            // if two tabs start an attempt at the same moment.
            builder.HasIndex(x => new { x.TrainingEnrollmentId, x.AssessmentId, x.AttemptNumber }).IsUnique();
        }
    }

    public class AttemptAnswerConfiguration : IEntityTypeConfiguration<AttemptAnswer>
    {
        public void Configure(EntityTypeBuilder<AttemptAnswer> builder)
        {
            builder.ToTable("AttemptAnswer", "Hrms");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.PointsAwarded).HasPrecision(6, 2);

            builder.HasOne<Question>()
                .WithMany()
                .HasForeignKey(x => x.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Selected)
                .WithOne()
                .HasForeignKey(o => o.AttemptAnswerId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(x => x.Selected).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(x => new { x.AssessmentAttemptId, x.QuestionId }).IsUnique();
        }
    }

    public class AttemptAnswerOptionConfiguration : IEntityTypeConfiguration<AttemptAnswerOption>
    {
        public void Configure(EntityTypeBuilder<AttemptAnswerOption> builder)
        {
            builder.ToTable("AttemptAnswerOption", "Hrms");
            builder.HasKey(x => x.Id);

            builder.HasOne<QuestionOption>()
                .WithMany()
                .HasForeignKey(x => x.QuestionOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.AttemptAnswerId, x.QuestionOptionId }).IsUnique();
        }
    }

    /// <summary>
    /// Course to competency — the mirror of PositionCompetencyConfiguration, and it uses the same
    /// delete rules for the same reasons: the mapping dies with its COURSE (cascade), while a
    /// competency that is in use anywhere cannot be deleted (restrict). Only one cascade path, so
    /// SQL Server raises no multiple-cascade-path error.
    /// </summary>
    public class CourseCompetencyConfiguration : IEntityTypeConfiguration<CourseCompetency>
    {
        public void Configure(EntityTypeBuilder<CourseCompetency> builder)
        {
            builder.ToTable("CourseCompetency", "Hrms");
            builder.HasKey(x => x.Id);

            builder.HasOne<TrainingCourse>()
                .WithMany()
                .HasForeignKey(x => x.TrainingCourseId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<Competency>()
                .WithMany()
                .HasForeignKey(x => x.CompetencyId)
                .OnDelete(DeleteBehavior.Restrict);

            // One row per pair — mapping the same competency twice says nothing extra and would
            // double-count the course in a recommendation list.
            builder.HasIndex(x => new { x.TrainingCourseId, x.CompetencyId }).IsUnique();
            // The recommendation query reads competency-first ("which courses teach X?").
            builder.HasIndex(x => x.CompetencyId);
        }
    }

    public class TrainingCourseConfiguration : IEntityTypeConfiguration<TrainingCourse>
    {
        public void Configure(EntityTypeBuilder<TrainingCourse> builder)
        {
            builder.ToTable("TrainingCourse", "Hrms");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Code).HasMaxLength(50);
            builder.Property(x => x.Description).HasMaxLength(2000);
            builder.Property(x => x.Objectives).HasMaxLength(2000);
            builder.Property(x => x.TargetAudience).HasMaxLength(500);
            builder.Property(x => x.Prerequisites).HasMaxLength(1000);
            builder.Property(x => x.DurationHours).HasPrecision(8, 2);
            builder.Property(x => x.DeliveryMode).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.CpdHours).HasPrecision(8, 2);
            builder.Property(x => x.ProviderName).HasMaxLength(200);
            builder.Property(x => x.ExternalUrl).HasMaxLength(500);

            builder.HasOne<TrainingCategory>()
                .WithMany()
                .HasForeignKey(x => x.TrainingCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
            builder.HasIndex(x => new { x.TenantId, x.TrainingCategoryId });
        }
    }

    public class TrainingSessionConfiguration : IEntityTypeConfiguration<TrainingSession>
    {
        public void Configure(EntityTypeBuilder<TrainingSession> builder)
        {
            builder.ToTable("TrainingSession", "Hrms");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Venue).HasMaxLength(300);
            builder.Property(x => x.TrainerType).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.TrainerName).HasMaxLength(200);
            builder.Property(x => x.ProviderName).HasMaxLength(200);
            builder.Property(x => x.MeetingUrl).HasMaxLength(500);
            builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.TrainerCost).HasPrecision(18, 2);
            builder.Property(x => x.MaterialsCost).HasPrecision(18, 2);
            builder.Property(x => x.VenueCost).HasPrecision(18, 2);
            builder.Property(x => x.Notes).HasMaxLength(1000);

            builder.Ignore(x => x.TotalCost);

            builder.HasOne<TrainingCourse>()
                .WithMany()
                .HasForeignKey(x => x.TrainingCourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Serves the schedule list and the budget-utilization year SUM.
            builder.HasIndex(x => new { x.TenantId, x.StartDate });
            builder.HasIndex(x => new { x.TenantId, x.TrainingCourseId });
        }
    }

    public class TrainingEnrollmentConfiguration : IEntityTypeConfiguration<TrainingEnrollment>
    {
        public void Configure(EntityTypeBuilder<TrainingEnrollment> builder)
        {
            builder.ToTable("TrainingEnrollment", "Hrms");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.AttendancePercent).HasPrecision(5, 2);
            builder.Property(x => x.AssessmentScore).HasPrecision(5, 2);
            builder.Property(x => x.FeedbackComments).HasMaxLength(2000);

            builder.HasOne<TrainingSession>()
                .WithMany()
                .HasForeignKey(x => x.TrainingSessionId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<TrainingNeed>()
                .WithMany()
                .HasForeignKey(x => x.TrainingNeedId)
                .OnDelete(DeleteBehavior.SetNull);

            // One enrollment per employee per session; employee-side list reads the second index.
            builder.HasIndex(x => new { x.TenantId, x.TrainingSessionId, x.EmployeeId }).IsUnique();
            builder.HasIndex(x => new { x.TenantId, x.EmployeeId, x.Status });
        }
    }

    public class TrainingBudgetConfiguration : IEntityTypeConfiguration<TrainingBudget>
    {
        public void Configure(EntityTypeBuilder<TrainingBudget> builder)
        {
            builder.ToTable("TrainingBudget", "Hrms");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Amount).HasPrecision(18, 2);
            builder.Property(x => x.Notes).HasMaxLength(1000);

            builder.HasOne<OrganizationUnit>()
                .WithMany()
                .HasForeignKey(x => x.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.TenantId, x.FiscalYear, x.OrganizationUnitId }).IsUnique();
        }
    }

    public class LearningPathConfiguration : IEntityTypeConfiguration<LearningPath>
    {
        public void Configure(EntityTypeBuilder<LearningPath> builder)
        {
            builder.ToTable("LearningPath", "Hrms");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Description).HasMaxLength(2000);

            builder.HasOne<Position>()
                .WithMany()
                .HasForeignKey(x => x.TargetPositionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Steps)
                .WithOne()
                .HasForeignKey(s => s.LearningPathId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(x => x.Steps).AutoInclude(false);

            builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
        }
    }

    public class LearningPathStepConfiguration : IEntityTypeConfiguration<LearningPathStep>
    {
        public void Configure(EntityTypeBuilder<LearningPathStep> builder)
        {
            builder.ToTable("LearningPathStep", "Hrms");
            builder.HasKey(x => x.Id);

            builder.HasOne<TrainingCourse>()
                .WithMany()
                .HasForeignKey(x => x.TrainingCourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.TenantId, x.LearningPathId });
        }
    }

    public class EmployeeTrainingCertificateConfiguration : IEntityTypeConfiguration<EmployeeTrainingCertificate>
    {
        public void Configure(EntityTypeBuilder<EmployeeTrainingCertificate> builder)
        {
            builder.ToTable("EmployeeTrainingCertificate", "Hrms");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CertificateNo).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Title).IsRequired().HasMaxLength(300);
            builder.Property(x => x.Notes).HasMaxLength(1000);

            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<TrainingCourse>()
                .WithMany()
                .HasForeignKey(x => x.TrainingCourseId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<TrainingEnrollment>()
                .WithMany()
                .HasForeignKey(x => x.TrainingEnrollmentId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(x => new { x.TenantId, x.CertificateNo }).IsUnique();
            builder.HasIndex(x => new { x.TenantId, x.EmployeeId });
            // Serves the renewal-tracking sweep (HC200): expiring-soon reads this ordered index.
            builder.HasIndex(x => new { x.TenantId, x.ExpiresOn });
        }
    }

    public class TrainingProviderPaymentConfiguration : IEntityTypeConfiguration<TrainingProviderPayment>
    {
        public void Configure(EntityTypeBuilder<TrainingProviderPayment> builder)
        {
            builder.ToTable("TrainingProviderPayment", "Hrms");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ProviderName).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Amount).HasPrecision(18, 2);
            builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.Reference).HasMaxLength(200);
            builder.Property(x => x.Notes).HasMaxLength(1000);

            builder.HasOne<TrainingSession>()
                .WithMany()
                .HasForeignKey(x => x.TrainingSessionId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(x => new { x.TenantId, x.Status });
            builder.HasIndex(x => new { x.TenantId, x.TrainingSessionId });
        }
    }

    public class LearningCommunityConfiguration : IEntityTypeConfiguration<LearningCommunity>
    {
        public void Configure(EntityTypeBuilder<LearningCommunity> builder)
        {
            builder.ToTable("LearningCommunity", "Hrms");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Description).HasMaxLength(1000);
            builder.Property(x => x.Kind).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.Tags).HasMaxLength(300);

            builder.HasOne<TrainingCourse>()
                .WithMany()
                .HasForeignKey(x => x.TrainingCourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
        }
    }

    public class CommunityPostReactionConfiguration : IEntityTypeConfiguration<CommunityPostReaction>
    {
        public void Configure(EntityTypeBuilder<CommunityPostReaction> builder)
        {
            builder.ToTable("CommunityPostReaction", "Hrms");
            builder.HasKey(x => x.Id);

            builder.HasOne<LearningCommunityPost>()
                .WithMany()
                .HasForeignKey(x => x.LearningCommunityPostId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // One reaction per employee per post — reacting again toggles off.
            builder.HasIndex(x => new { x.TenantId, x.LearningCommunityPostId, x.EmployeeId }).IsUnique();
        }
    }

    public class LearningCommunityMemberConfiguration : IEntityTypeConfiguration<LearningCommunityMember>
    {
        public void Configure(EntityTypeBuilder<LearningCommunityMember> builder)
        {
            builder.ToTable("LearningCommunityMember", "Hrms");
            builder.HasKey(x => x.Id);

            builder.HasOne<LearningCommunity>()
                .WithMany()
                .HasForeignKey(x => x.LearningCommunityId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.TenantId, x.LearningCommunityId, x.EmployeeId }).IsUnique();
        }
    }

    public class LearningCommunityPostConfiguration : IEntityTypeConfiguration<LearningCommunityPost>
    {
        public void Configure(EntityTypeBuilder<LearningCommunityPost> builder)
        {
            builder.ToTable("LearningCommunityPost", "Hrms");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Content).IsRequired().HasMaxLength(4000);
            // ParentPostId stays FK-less by design (see the entity remark).

            builder.HasOne<LearningCommunity>()
                .WithMany()
                .HasForeignKey(x => x.LearningCommunityId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Serves the thread feed (topics newest-first) and reply lookups.
            builder.HasIndex(x => new { x.TenantId, x.LearningCommunityId, x.ParentPostId });
        }
    }

    public class TrainingNeedConfiguration : IEntityTypeConfiguration<TrainingNeed>
    {
        public void Configure(EntityTypeBuilder<TrainingNeed> builder)
        {
            builder.ToTable("TrainingNeed", "Hrms");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Topic).IsRequired().HasMaxLength(300);
            builder.Property(x => x.Justification).IsRequired().HasMaxLength(2000);
            builder.Property(x => x.NeedType).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.Priority).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.Source).IsRequired().HasConversion<string>().HasMaxLength(30);
            builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(30);
            builder.Property(x => x.EstimatedCost).HasPrecision(18, 2);

            builder.Ignore(x => x.WorkflowEntityType);

            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<TrainingCourse>()
                .WithMany()
                .HasForeignKey(x => x.TrainingCourseId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Competency>()
                .WithMany()
                .HasForeignKey(x => x.CompetencyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.TenantId, x.Status });
            builder.HasIndex(x => new { x.TenantId, x.EmployeeId });
        }
    }
}
