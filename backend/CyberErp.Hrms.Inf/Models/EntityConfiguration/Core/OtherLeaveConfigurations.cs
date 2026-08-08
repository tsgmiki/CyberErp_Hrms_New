using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    // ===== Other (non-annual) Leave — static, position-based, gender-aware entitlements =====

    /// <summary>Static per-fiscal-year policy (hrmsOtherLeaveSetting) — no accrual, ever.</summary>
    public class OtherLeaveSettingConfiguration : IEntityTypeConfiguration<OtherLeaveSetting>
    {
        public void Configure(EntityTypeBuilder<OtherLeaveSetting> builder)
        {
            builder.ToTable("OtherLeaveSetting", "Hrms");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Gender).HasConversion<string>().HasMaxLength(10).IsRequired();
            builder.Property(x => x.DayCounting).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.StandardDays).HasPrecision(6, 2);
            builder.Property(x => x.ManagerialDays).HasPrecision(6, 2);
            builder.Property(x => x.Description).HasMaxLength(1000);

            builder.HasOne(x => x.FiscalYear).WithMany().HasForeignKey(x => x.FiscalYearId).OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(x => x.FiscalYear).UsePropertyAccessMode(PropertyAccessMode.Field);
            // The LeaveType master relationship (moved here from hrmsAnnualLeaveSetting).
            builder.HasOne(x => x.LeaveType).WithMany().HasForeignKey(x => x.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(x => x.LeaveType).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(x => new { x.TenantId, x.FiscalYearId, x.LeaveTypeId }).IsUnique();
            builder.HasIndex(x => new { x.TenantId, x.IsActive });
        }
    }

    /// <summary>Request header (hrmsOtherLeave) — mirrors hrmsAnnualLeaveHeader.</summary>
    public class OtherLeaveHeaderConfiguration : IEntityTypeConfiguration<OtherLeaveHeader>
    {
        public void Configure(EntityTypeBuilder<OtherLeaveHeader> builder)
        {
            builder.ToTable("OtherLeave", "Hrms");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.RequestDate).HasColumnType("date");
            builder.Property(x => x.Remark).HasMaxLength(1000);
            builder.Property(x => x.TotalLeaveDays).HasPrecision(6, 2);
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

            builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Setting).WithMany().HasForeignKey(x => x.OtherLeaveSettingId).OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(x => x.Employee).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Navigation(x => x.Setting).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(x => x.Details).WithOne().HasForeignKey(d => d.OtherLeaveHeaderId).OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.EmployeeId);
            builder.HasIndex(x => x.OtherLeaveSettingId);
            builder.HasIndex(x => new { x.EmployeeId, x.Status });
        }
    }

    /// <summary>Detail row (hrmsOtherLeaveDetail) — one full-day date range.</summary>
    public class OtherLeaveDetailConfiguration : IEntityTypeConfiguration<OtherLeaveDetail>
    {
        public void Configure(EntityTypeBuilder<OtherLeaveDetail> builder)
        {
            builder.ToTable("OtherLeaveDetail", "Hrms");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.LeaveDays).HasPrecision(6, 2);
            builder.Property(x => x.StartDate).HasColumnType("date");
            builder.Property(x => x.EndDate).HasColumnType("date");

            builder.HasIndex(x => x.OtherLeaveHeaderId);
            builder.HasIndex(x => new { x.OtherLeaveHeaderId, x.StartDate, x.EndDate });
        }
    }
}
