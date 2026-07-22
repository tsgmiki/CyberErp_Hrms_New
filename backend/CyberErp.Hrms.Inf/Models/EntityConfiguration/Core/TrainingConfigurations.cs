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
            builder.ToTable("hrmsTrainingCategory", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
            builder.Property(x => x.Description).HasMaxLength(1000);

            builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
        }
    }

    public class TrainingCourseConfiguration : IEntityTypeConfiguration<TrainingCourse>
    {
        public void Configure(EntityTypeBuilder<TrainingCourse> builder)
        {
            builder.ToTable("hrmsTrainingCourse", "dbo");
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
            builder.ToTable("hrmsTrainingSession", "dbo");
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
            builder.ToTable("hrmsTrainingEnrollment", "dbo");
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
            builder.ToTable("hrmsTrainingBudget", "dbo");
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
            builder.ToTable("hrmsLearningPath", "dbo");
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
            builder.ToTable("hrmsLearningPathStep", "dbo");
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
            builder.ToTable("hrmsEmployeeTrainingCertificate", "dbo");
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
            builder.ToTable("hrmsTrainingProviderPayment", "dbo");
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
            builder.ToTable("hrmsLearningCommunity", "dbo");
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
            builder.ToTable("hrmsCommunityPostReaction", "dbo");
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
            builder.ToTable("hrmsLearningCommunityMember", "dbo");
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
            builder.ToTable("hrmsLearningCommunityPost", "dbo");
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
            builder.ToTable("hrmsTrainingNeed", "dbo");
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
