using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class ReportConfiguration : IEntityTypeConfiguration<Report>
    {
        public void Configure(EntityTypeBuilder<Report> builder)
        {
            builder.ToTable("hrmsReport", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ReportKey).IsRequired().HasMaxLength(100);
            builder.Property(x => x.ReportName).IsRequired().HasMaxLength(200);
            builder.Property(x => x.ReportGrouping).IsRequired().HasMaxLength(150);
            builder.Property(x => x.StoredProc).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Description).HasMaxLength(1000);

            builder.HasMany(x => x.Fields).WithOne().HasForeignKey(f => f.ReportId).OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(x => x.Fields).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasMany(x => x.FieldOutputs).WithOne().HasForeignKey(f => f.ReportId).OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(x => x.FieldOutputs).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasMany(x => x.Restrictions).WithOne().HasForeignKey(f => f.ReportId).OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(x => x.Restrictions).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(x => new { x.TenantId, x.ReportKey }).IsUnique();
            builder.HasIndex(x => new { x.TenantId, x.IsActive });
        }
    }

    public class ReportFieldOutputConfiguration : IEntityTypeConfiguration<ReportFieldOutput>
    {
        public void Configure(EntityTypeBuilder<ReportFieldOutput> builder)
        {
            builder.ToTable("hrmsReportFieldOutput", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Field).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Label).IsRequired().HasMaxLength(200);
            builder.HasIndex(x => x.ReportId);
        }
    }

    public class SavedReportFilterConfiguration : IEntityTypeConfiguration<SavedReportFilter>
    {
        public void Configure(EntityTypeBuilder<SavedReportFilter> builder)
        {
            builder.ToTable("hrmsReportSavedFilter", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.CriteriaJson).IsRequired();
            builder.HasOne<Report>().WithMany().HasForeignKey(x => x.ReportId).OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(x => x.ReportId);
        }
    }

    public class ReportRestrictionConfiguration : IEntityTypeConfiguration<ReportRestriction>
    {
        public void Configure(EntityTypeBuilder<ReportRestriction> builder)
        {
            builder.ToTable("hrmsReportRestriction", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.RoleName).IsRequired().HasMaxLength(200);
            builder.HasIndex(x => x.ReportId);
            builder.HasIndex(x => x.RoleId);
        }
    }

    public class ReportScheduleConfiguration : IEntityTypeConfiguration<ReportSchedule>
    {
        public void Configure(EntityTypeBuilder<ReportSchedule> builder)
        {
            builder.ToTable("hrmsReportSchedule", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.CronExpression).IsRequired().HasMaxLength(100);
            builder.Property(x => x.MailSubject).HasMaxLength(300);
            builder.Property(x => x.Frequency).IsRequired().HasMaxLength(20);
            builder.HasOne<Report>().WithMany().HasForeignKey(x => x.ReportId).OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(x => x.ReportId);
        }
    }

    public class ReportScheduleRecipientConfiguration : IEntityTypeConfiguration<ReportScheduleRecipient>
    {
        public void Configure(EntityTypeBuilder<ReportScheduleRecipient> builder)
        {
            builder.ToTable("hrmsReportScheduleRecipient", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Email).HasMaxLength(300);
            builder.HasOne<ReportSchedule>().WithMany().HasForeignKey(x => x.ReportScheduleId).OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(x => x.ReportScheduleId);
        }
    }

    public class ReportScheduleFieldValueConfiguration : IEntityTypeConfiguration<ReportScheduleFieldValue>
    {
        public void Configure(EntityTypeBuilder<ReportScheduleFieldValue> builder)
        {
            builder.ToTable("hrmsReportScheduleFieldValue", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ReportKey).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Field).IsRequired().HasMaxLength(100);
            builder.HasOne<ReportSchedule>().WithMany().HasForeignKey(x => x.ReportScheduleId).OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(x => x.ReportScheduleId);
        }
    }

    public class ReportScheduleFieldOutputConfiguration : IEntityTypeConfiguration<ReportScheduleFieldOutput>
    {
        public void Configure(EntityTypeBuilder<ReportScheduleFieldOutput> builder)
        {
            builder.ToTable("hrmsReportScheduleFieldOutput", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ReportKey).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Field).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Label).IsRequired().HasMaxLength(200);
            builder.HasOne<ReportSchedule>().WithMany().HasForeignKey(x => x.ReportScheduleId).OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(x => x.ReportScheduleId);
        }
    }

    public class ReportRunConfiguration : IEntityTypeConfiguration<ReportRun>
    {
        public void Configure(EntityTypeBuilder<ReportRun> builder)
        {
            builder.ToTable("hrmsReportRun", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ReportKey).IsRequired().HasMaxLength(100);
            builder.Property(x => x.CriteriaJson).IsRequired();
            builder.Property(x => x.RanBy).HasMaxLength(200);
            builder.HasIndex(x => new { x.TenantId, x.ReportKey });
        }
    }

    public class ReportRunRecipientConfiguration : IEntityTypeConfiguration<ReportRunRecipient>
    {
        public void Configure(EntityTypeBuilder<ReportRunRecipient> builder)
        {
            builder.ToTable("hrmsReportRunRecipient", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Email).IsRequired().HasMaxLength(300);
            builder.HasOne<ReportRun>().WithMany().HasForeignKey(x => x.ReportRunId).OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(x => x.ReportRunId);
        }
    }

    public class ReportFieldConfiguration : IEntityTypeConfiguration<ReportField>
    {
        public void Configure(EntityTypeBuilder<ReportField> builder)
        {
            builder.ToTable("hrmsReportField", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Field).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Label).IsRequired().HasMaxLength(200);
            builder.Property(x => x.DataType).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.DependencyField).HasMaxLength(100);

            builder.HasIndex(x => x.ReportId);
        }
    }
}
