using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class WorkShiftConfiguration : IEntityTypeConfiguration<WorkShift>
    {
        public void Configure(EntityTypeBuilder<WorkShift> builder)
        {
            builder.ToTable("WorkShift", "Hrms");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.NameA).HasMaxLength(200);

            // ⚠️ Computed from the stored times — EF must not try to map them to columns.
            builder.Ignore(x => x.CrossesMidnight);
            builder.Ignore(x => x.ScheduledMinutes);

            builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        }
    }

    public class EmployeeShiftAssignmentConfiguration : IEntityTypeConfiguration<EmployeeShiftAssignment>
    {
        public void Configure(EntityTypeBuilder<EmployeeShiftAssignment> builder)
        {
            builder.ToTable("EmployeeShiftAssignment", "Hrms");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Remark).HasMaxLength(500);

            builder.HasOne(x => x.Employee).WithMany()
                .HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkShift).WithMany()
                .HasForeignKey(x => x.WorkShiftId).OnDelete(DeleteBehavior.Restrict);

            // The processor's hot path: "which shift governed this employee on this date".
            builder.HasIndex(x => new { x.TenantId, x.EmployeeId, x.EffectiveFrom });
        }
    }

    public class AttendanceDeviceConfiguration : IEntityTypeConfiguration<AttendanceDevice>
    {
        public void Configure(EntityTypeBuilder<AttendanceDevice> builder)
        {
            builder.ToTable("AttendanceDevice", "Hrms");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            // ⚠️ A STRING, not an enum column — a new machine must not need a migration.
            builder.Property(x => x.Protocol).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Location).HasMaxLength(200);
            builder.Property(x => x.Endpoint).HasMaxLength(300);
            builder.Property(x => x.SerialNumber).HasMaxLength(100);
            builder.Property(x => x.IngestKey).HasMaxLength(200);
            builder.Property(x => x.LastSyncError).HasMaxLength(1000);

            builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        }
    }

    public class AttendanceEnrollmentConfiguration : IEntityTypeConfiguration<AttendanceEnrollment>
    {
        public void Configure(EntityTypeBuilder<AttendanceEnrollment> builder)
        {
            builder.ToTable("AttendanceEnrollment", "Hrms");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.DeviceUserId).IsRequired().HasMaxLength(100);

            builder.HasOne(x => x.AttendanceDevice).WithMany()
                .HasForeignKey(x => x.AttendanceDeviceId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Employee).WithMany()
                .HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);

            // ⚠️ One slot on one machine belongs to ONE person. Without this, two employees can be
            // enrolled as user 37 on the same door and every punch becomes ambiguous.
            builder.HasIndex(x => new { x.TenantId, x.AttendanceDeviceId, x.DeviceUserId }).IsUnique();
        }
    }

    public class AttendancePunchConfiguration : IEntityTypeConfiguration<AttendancePunch>
    {
        public void Configure(EntityTypeBuilder<AttendancePunch> builder)
        {
            builder.ToTable("AttendancePunch", "Hrms");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.DeviceUserId).HasMaxLength(100);
            builder.Property(x => x.ExternalId).HasMaxLength(100);
            builder.Property(x => x.RawPayload).HasMaxLength(2000);
            builder.Property(x => x.UnresolvedReason).HasMaxLength(500);
            builder.Property(x => x.Direction).HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.Source).HasConversion<string>().HasMaxLength(20);

            builder.HasOne(x => x.Employee).WithMany()
                .HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AttendanceDevice).WithMany()
                .HasForeignKey(x => x.AttendanceDeviceId).OnDelete(DeleteBehavior.Restrict);

            // ⚠️ THE RE-READ GUARD. A terminal's log pulled twice reports the same external ids, so
            // this unique index turns the second pull into a no-op instead of doubling everybody's
            // day. Filtered, because a manual punch has no external id and many rows share NULL —
            // SQL Server treats NULLs as equal in a unique index unless they are filtered out.
            builder.HasIndex(x => new { x.TenantId, x.AttendanceDeviceId, x.ExternalId })
                .IsUnique()
                .HasFilter("[ExternalId] IS NOT NULL AND [AttendanceDeviceId] IS NOT NULL");

            builder.HasIndex(x => new { x.TenantId, x.EmployeeId, x.PunchedAt });
        }
    }

    public class AttendanceDayConfiguration : IEntityTypeConfiguration<AttendanceDay>
    {
        public void Configure(EntityTypeBuilder<AttendanceDay> builder)
        {
            builder.ToTable("AttendanceDay", "Hrms");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.DerivedStatus).HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.DayValue).HasPrecision(4, 2);
            builder.Property(x => x.OverrideReason).HasMaxLength(500);
            builder.Property(x => x.OverriddenBy).HasMaxLength(200);
            builder.Property(x => x.Remark).HasMaxLength(500);

            builder.HasOne(x => x.Employee).WithMany()
                .HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkShift).WithMany()
                .HasForeignKey(x => x.WorkShiftId).OnDelete(DeleteBehavior.SetNull);

            // ⚠️ One row per employee per date. The processor upserts against this, so running a
            // day twice settles the same row instead of stacking duplicates nobody can reconcile.
            builder.HasIndex(x => new { x.TenantId, x.EmployeeId, x.WorkDate }).IsUnique();
            builder.HasIndex(x => new { x.TenantId, x.WorkDate, x.Status });
        }
    }
}
