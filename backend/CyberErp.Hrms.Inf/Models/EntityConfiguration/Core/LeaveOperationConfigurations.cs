using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
    {
        public void Configure(EntityTypeBuilder<LeaveRequest> builder)
        {
            builder.ToTable("hrms_LeaveRequest", "Core");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.DayPart).HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.WorkingDays).HasPrecision(6, 2);
            builder.Property(x => x.Reason).HasMaxLength(1000);
            builder.Property(x => x.DecisionComment).HasMaxLength(1000);
            builder.Property(x => x.CancelReason).HasMaxLength(1000);

            builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.LeaveType).WithMany().HasForeignKey(x => x.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<FiscalYear>().WithMany().HasForeignKey(x => x.FiscalYearId).OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(x => x.Employee).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Navigation(x => x.LeaveType).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(x => x.EmployeeId);
            builder.HasIndex(x => new { x.EmployeeId, x.Status });
            builder.HasIndex(x => x.FiscalYearId);
        }
    }

    public class LeaveBalanceConfiguration : IEntityTypeConfiguration<LeaveBalance>
    {
        public void Configure(EntityTypeBuilder<LeaveBalance> builder)
        {
            builder.ToTable("hrms_LeaveBalance", "Core");
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
            builder.ToTable("hrms_LeaveBalanceTransaction", "Core");
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
