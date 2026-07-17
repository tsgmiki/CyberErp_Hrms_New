using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class InterviewConfiguration : IEntityTypeConfiguration<Interview>
    {
        public void Configure(EntityTypeBuilder<Interview> builder)
        {
            builder.ToTable("hrmsInterview", "dbo",
                // Defense-in-depth beyond FluentValidation (logic.md §7.1 adoption #3).
                t => t.HasCheckConstraint("CK_hrms_Interview_Window", "[ScheduledEnd] > [ScheduledStart]"));

            builder.HasKey(i => i.Id);

            builder.Property(i => i.Format).IsRequired().HasConversion<string>().HasMaxLength(30);
            builder.Property(i => i.Status).IsRequired().HasConversion<string>().HasMaxLength(30);
            builder.Property(i => i.Location).HasMaxLength(300);
            builder.Property(i => i.MeetingLink).HasMaxLength(500);
            builder.Property(i => i.Notes).HasMaxLength(2000);

            builder.HasOne<JobApplication>()
                .WithMany()
                .HasForeignKey(i => i.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(i => i.Panelists)
                .WithOne()
                .HasForeignKey(p => p.InterviewId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(i => i.Panelists).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(i => i.ApplicationId);
            builder.HasIndex(i => new { i.TenantId, i.Status });
            builder.HasIndex(i => i.ScheduledStart);
        }
    }

    public class InterviewPanelistConfiguration : IEntityTypeConfiguration<InterviewPanelist>
    {
        public void Configure(EntityTypeBuilder<InterviewPanelist> builder)
        {
            builder.ToTable("hrmsInterviewPanelist", "dbo");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.PanelistName).IsRequired().HasMaxLength(300);
            builder.Property(p => p.Attendance).IsRequired().HasConversion<string>().HasMaxLength(30);

            // The panel assignment must not block employee deletion (name snapshot survives).
            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(p => p.Feedback)
                .WithOne()
                .HasForeignKey(f => f.PanelistId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(p => p.Feedback).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(p => new { p.InterviewId, p.EmployeeId }).IsUnique()
                .HasFilter("[EmployeeId] IS NOT NULL");
            builder.HasIndex(p => p.EmployeeId);
        }
    }

    public class InterviewFeedbackConfiguration : IEntityTypeConfiguration<InterviewFeedback>
    {
        public void Configure(EntityTypeBuilder<InterviewFeedback> builder)
        {
            builder.ToTable("hrmsInterviewFeedback", "dbo",
                t => t.HasCheckConstraint("CK_hrms_InterviewFeedback_Score", "[Score] >= 0 AND [Score] <= 100"));

            builder.HasKey(f => f.Id);

            builder.Property(f => f.Score).HasColumnType("decimal(5,2)");
            builder.Property(f => f.CriterionName).HasMaxLength(300);
            builder.Property(f => f.Comments).HasMaxLength(2000);

            // Criterion reference kept loose (no FK) — criteria are replaced wholesale on
            // requisition edits; the row snapshots the criterion name (same as ApplicationCriterionScore).
            builder.HasIndex(f => new { f.PanelistId, f.CriterionId }).IsUnique()
                .HasFilter("[CriterionId] IS NOT NULL");
            builder.HasIndex(f => f.PanelistId);
        }
    }

    public class JobOfferConfiguration : IEntityTypeConfiguration<JobOffer>
    {
        public void Configure(EntityTypeBuilder<JobOffer> builder)
        {
            builder.ToTable("hrmsJobOffer", "dbo",
                t => t.HasCheckConstraint("CK_hrms_JobOffer_Salary", "[Salary] > 0"));

            builder.HasKey(o => o.Id);

            builder.Property(o => o.OfferNumber).IsRequired().HasMaxLength(30);
            builder.Property(o => o.Status).IsRequired().HasConversion<string>().HasMaxLength(30);
            builder.Property(o => o.Salary).HasColumnType("decimal(18,2)");
            builder.Property(o => o.SalaryJustification).HasMaxLength(1000);
            builder.Property(o => o.HiringManagerName).HasMaxLength(300);
            builder.Property(o => o.ResponseNote).HasMaxLength(1000);
            builder.Property(o => o.LetterText).HasMaxLength(8000);

            builder.HasOne<JobApplication>()
                .WithMany()
                .HasForeignKey(o => o.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(o => o.HiringManagerEmployeeId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne<SalaryScale>()
                .WithMany()
                .HasForeignKey(o => o.SalaryScaleId)
                .OnDelete(DeleteBehavior.Restrict);

            // HiredEmployeeId is a historical pointer (no FK — cascade-path limit, cf. Candidate).
            builder.HasIndex(o => o.HiredEmployeeId);

            builder.HasIndex(o => new { o.TenantId, o.OfferNumber }).IsUnique();
            builder.HasIndex(o => new { o.TenantId, o.Status });
            // At most one ACTIVE offer per application (enum stored as string → filtered index).
            builder.HasIndex(o => o.ApplicationId).IsUnique()
                .HasDatabaseName("IX_hrms_JobOffer_Application_Active")
                .HasFilter("[Status] IN ('Draft','PendingApproval','Approved','Sent')");
            // Latest-offer-per-application lookups (ranking/eligibility) scan ALL statuses — the
            // filtered active-only index above cannot serve them.
            builder.HasIndex(o => new { o.ApplicationId, o.CreatedAt });
        }
    }

    /// <summary>
    /// Per-tenant atomic counter (logic.md §7.1 adoption #5). Not a BaseEntity — no audit columns,
    /// no rowversion, no Finbuckle attribute: rows are managed exclusively by atomic raw SQL in
    /// NumberSequenceService with an explicit TenantId predicate.
    /// </summary>
    public class NumberSequenceConfiguration : IEntityTypeConfiguration<NumberSequence>
    {
        public void Configure(EntityTypeBuilder<NumberSequence> builder)
        {
            builder.ToTable("hrmsNumberSequence", "dbo");

            builder.HasKey(s => new { s.TenantId, s.Key });
            builder.Property(s => s.TenantId).HasMaxLength(64);
            builder.Property(s => s.Key).HasMaxLength(50);
        }
    }
}
