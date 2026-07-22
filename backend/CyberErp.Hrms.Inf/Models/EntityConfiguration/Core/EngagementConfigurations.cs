using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    // Employee Engagement — §3.9.1 (HC203/HC205/HC206/HC207): suggestions, grievances, announcements.

    public class SuggestionConfiguration : IEntityTypeConfiguration<Suggestion>
    {
        public void Configure(EntityTypeBuilder<Suggestion> builder)
        {
            builder.ToTable("hrmsSuggestion", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title).IsRequired().HasMaxLength(300);
            builder.Property(x => x.Body).IsRequired().HasMaxLength(4000);
            builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.ManagementResponse).HasMaxLength(2000);

            // No Employee FK: anonymous rows have no link at all, and named rows keep a plain id
            // so a submitter's later offboarding never blocks the suggestion box.
            builder.HasIndex(x => new { x.TenantId, x.Status });
        }
    }

    public class GrievanceConfiguration : IEntityTypeConfiguration<Grievance>
    {
        public void Configure(EntityTypeBuilder<Grievance> builder)
        {
            builder.ToTable("hrmsGrievance", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Category).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Subject).IsRequired().HasMaxLength(300);
            builder.Property(x => x.Details).IsRequired().HasMaxLength(4000);
            builder.Property(x => x.Severity).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.Resolution).HasMaxLength(2000);

            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Notes)
                .WithOne()
                .HasForeignKey(n => n.GrievanceId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(x => x.Notes).AutoInclude(false);

            builder.HasIndex(x => new { x.TenantId, x.Status });
            builder.HasIndex(x => new { x.TenantId, x.EmployeeId });
            builder.HasIndex(x => new { x.TenantId, x.AssignedToEmployeeId });
        }
    }

    public class GrievanceNoteConfiguration : IEntityTypeConfiguration<GrievanceNote>
    {
        public void Configure(EntityTypeBuilder<GrievanceNote> builder)
        {
            builder.ToTable("hrmsGrievanceNote", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Note).IsRequired().HasMaxLength(2000);

            builder.HasIndex(x => new { x.TenantId, x.GrievanceId });
        }
    }

    public class SurveyConfiguration : IEntityTypeConfiguration<Survey>
    {
        public void Configure(EntityTypeBuilder<Survey> builder)
        {
            builder.ToTable("hrmsSurvey", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title).IsRequired().HasMaxLength(300);
            builder.Property(x => x.Description).HasMaxLength(1000);
            builder.Property(x => x.QuestionsJson).IsRequired();
            builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);

            builder.HasIndex(x => new { x.TenantId, x.Status });
        }
    }

    public class SurveyResponseConfiguration : IEntityTypeConfiguration<SurveyResponse>
    {
        public void Configure(EntityTypeBuilder<SurveyResponse> builder)
        {
            builder.ToTable("hrmsSurveyResponse", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.AnswersJson).IsRequired();

            builder.HasOne<Survey>()
                .WithMany()
                .HasForeignKey(x => x.SurveyId)
                .OnDelete(DeleteBehavior.Cascade);
            // Deliberately NO Employee FK — anonymous rows have no link, named rows keep a plain id.

            builder.HasIndex(x => new { x.TenantId, x.SurveyId });
        }
    }

    public class SurveyCompletionConfiguration : IEntityTypeConfiguration<SurveyCompletion>
    {
        public void Configure(EntityTypeBuilder<SurveyCompletion> builder)
        {
            builder.ToTable("hrmsSurveyCompletion", "dbo");
            builder.HasKey(x => x.Id);

            builder.HasOne<Survey>()
                .WithMany()
                .HasForeignKey(x => x.SurveyId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // One response per employee per survey — the double-vote guard.
            builder.HasIndex(x => new { x.TenantId, x.SurveyId, x.EmployeeId }).IsUnique();
        }
    }

    public class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
    {
        public void Configure(EntityTypeBuilder<Announcement> builder)
        {
            builder.ToTable("hrmsAnnouncement", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title).IsRequired().HasMaxLength(300);
            builder.Property(x => x.Body).IsRequired().HasMaxLength(8000);
            builder.Property(x => x.Audience).IsRequired().HasConversion<string>().HasMaxLength(20);

            builder.HasOne<Branch>()
                .WithMany()
                .HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<OrganizationUnit>()
                .WithMany()
                .HasForeignKey(x => x.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            // Serves the employee feed: active rows in their publish window, pinned first.
            builder.HasIndex(x => new { x.TenantId, x.IsActive, x.PublishFrom });
        }
    }
}
