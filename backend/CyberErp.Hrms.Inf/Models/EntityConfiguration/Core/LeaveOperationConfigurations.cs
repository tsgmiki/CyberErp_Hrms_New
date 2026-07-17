using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
    {
        public void Configure(EntityTypeBuilder<LeaveRequest> builder)
        {
            builder.ToTable("hrmsLeaveRequest", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.TotalWorkingDays).HasPrecision(6, 2);
            builder.Property(x => x.Reason).HasMaxLength(1000);
            builder.Property(x => x.DecisionComment).HasMaxLength(1000);
            builder.Property(x => x.CancelReason).HasMaxLength(1000);

            builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<FiscalYear>().WithMany().HasForeignKey(x => x.FiscalYearId).OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(x => x.Employee).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(x => x.Lines).WithOne().HasForeignKey(l => l.LeaveRequestId).OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.EmployeeId);
            builder.HasIndex(x => new { x.EmployeeId, x.Status });
            builder.HasIndex(x => x.FiscalYearId);
        }
    }

    public class LeaveRequestLineConfiguration : IEntityTypeConfiguration<LeaveRequestLine>
    {
        public void Configure(EntityTypeBuilder<LeaveRequestLine> builder)
        {
            builder.ToTable("hrmsLeaveRequestLine", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.DayPart).HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.WorkingDays).HasPrecision(6, 2);

            builder.HasOne(x => x.LeaveType).WithMany().HasForeignKey(x => x.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(x => x.LeaveType).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(x => x.LeaveRequestId);
            builder.HasIndex(x => x.LeaveTypeId);
        }
    }

    public class WorkWeekConfigurationConfiguration : IEntityTypeConfiguration<WorkWeekConfiguration>
    {
        public void Configure(EntityTypeBuilder<WorkWeekConfiguration> builder)
        {
            builder.ToTable("hrmsWorkWeekConfiguration", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
            foreach (var day in new[] { nameof(WorkWeekConfiguration.Monday), nameof(WorkWeekConfiguration.Tuesday),
                nameof(WorkWeekConfiguration.Wednesday), nameof(WorkWeekConfiguration.Thursday), nameof(WorkWeekConfiguration.Friday),
                nameof(WorkWeekConfiguration.Saturday), nameof(WorkWeekConfiguration.Sunday) })
            {
                builder.Property(day).HasConversion<string>().HasMaxLength(10).IsRequired();
            }

            builder.HasIndex(x => new { x.TenantId, x.IsActive });
        }
    }

    public class AnnualLeaveHeaderConfiguration : IEntityTypeConfiguration<AnnualLeaveHeader>
    {
        public void Configure(EntityTypeBuilder<AnnualLeaveHeader> builder)
        {
            builder.ToTable("hrmsAnnualLeaveHeader", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.RequestDate).HasColumnType("date");
            builder.Property(x => x.Remark).HasMaxLength(1000);
            builder.Property(x => x.TotalLeaveDays).HasPrecision(6, 2);
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

            builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            // AnnualLeaveLedgerId targets the annual-leave entitlement row (hrms_LeaveBalance).
            builder.HasOne(x => x.Ledger).WithMany().HasForeignKey(x => x.AnnualLeaveLedgerId).OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(x => x.Employee).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Navigation(x => x.Ledger).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(x => x.Details).WithOne().HasForeignKey(d => d.AnnualLeaveHeaderId).OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.EmployeeId);
            builder.HasIndex(x => x.AnnualLeaveLedgerId);
            builder.HasIndex(x => new { x.EmployeeId, x.Status });
        }
    }

    public class AnnualLeaveDetailConfiguration : IEntityTypeConfiguration<AnnualLeaveDetail>
    {
        public void Configure(EntityTypeBuilder<AnnualLeaveDetail> builder)
        {
            builder.ToTable("hrmsAnnualLeaveDetail", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.LeaveUsage).HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.HalfDayPart).HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.LeaveDays).HasPrecision(6, 2);
            builder.Property(x => x.StartDate).HasColumnType("date");
            builder.Property(x => x.EndDate).HasColumnType("date");

            builder.HasIndex(x => x.AnnualLeaveHeaderId);
            builder.HasIndex(x => new { x.AnnualLeaveHeaderId, x.StartDate, x.EndDate });
        }
    }

    public class LeaveBalanceConfiguration : IEntityTypeConfiguration<LeaveBalance>
    {
        public void Configure(EntityTypeBuilder<LeaveBalance> builder)
        {
            builder.ToTable("hrmsLeaveBalance", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Entitled).HasPrecision(8, 2);
            builder.Property(x => x.CarriedForward).HasPrecision(8, 2);
            builder.Property(x => x.Adjusted).HasPrecision(8, 2);
            builder.Property(x => x.Taken).HasPrecision(8, 2);
            builder.Ignore(x => x.Available);

            builder.HasOne(x => x.LeaveType).WithMany().HasForeignKey(x => x.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.FiscalYear).WithMany().HasForeignKey(x => x.FiscalYearId).OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(x => x.LeaveType).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Navigation(x => x.FiscalYear).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasOne<Employee>().WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);

            // One balance row per employee / leave type / fiscal year.
            builder.HasIndex(x => new { x.TenantId, x.EmployeeId, x.LeaveTypeId, x.FiscalYearId }).IsUnique();
        }
    }

    public class LeaveBalanceTransactionConfiguration : IEntityTypeConfiguration<LeaveBalanceTransaction>
    {
        public void Configure(EntityTypeBuilder<LeaveBalanceTransaction> builder)
        {
            builder.ToTable("hrmsLeaveBalanceTransaction", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TransactionType).HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.Delta).HasPrecision(8, 2);
            builder.Property(x => x.BalanceAfter).HasPrecision(8, 2);
            builder.Property(x => x.Reason).HasMaxLength(500);

            builder.HasIndex(x => new { x.EmployeeId, x.LeaveTypeId, x.FiscalYearId });
            builder.HasIndex(x => x.ReferenceId);
        }
    }
}
