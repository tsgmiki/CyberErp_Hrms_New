using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class PerDiemRateConfiguration : IEntityTypeConfiguration<PerDiemRate>
    {
        public void Configure(EntityTypeBuilder<PerDiemRate> builder)
        {
            builder.ToTable("hrmsPerDiemRate", "dbo");
            builder.HasKey(r => r.Id);

            builder.Property(r => r.TripType).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(r => r.DailyRate).HasColumnType("decimal(18,2)");
            builder.Property(r => r.Currency).IsRequired().HasMaxLength(10);
            builder.Property(r => r.IsActive).HasDefaultValue(true);

            builder.HasOne<JobGrade>().WithMany().HasForeignKey(r => r.JobGradeId).OnDelete(DeleteBehavior.Restrict);
            // One rate per grade per trip type per tenant.
            builder.HasIndex(r => new { r.TenantId, r.JobGradeId, r.TripType }).IsUnique();
        }
    }

    public class TripBudgetConfiguration : IEntityTypeConfiguration<TripBudget>
    {
        public void Configure(EntityTypeBuilder<TripBudget> builder)
        {
            builder.ToTable("hrmsTripBudget", "dbo");
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Amount).HasColumnType("decimal(18,2)");
            builder.Property(b => b.Notes).HasMaxLength(1000);

            builder.HasOne<OrganizationUnit>().WithMany().HasForeignKey(b => b.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            // One budget per (fiscal year, org unit) — SQL Server treats NULLs as equal, so a single org-wide row per year.
            builder.HasIndex(b => new { b.TenantId, b.FiscalYear, b.OrganizationUnitId }).IsUnique();
        }
    }

    public class TripRequestConfiguration : IEntityTypeConfiguration<TripRequest>
    {
        public void Configure(EntityTypeBuilder<TripRequest> builder)
        {
            builder.ToTable("hrmsTripRequest", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TripNumber).IsRequired().HasMaxLength(30);
            builder.Property(x => x.TripType).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.Destination).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Purpose).HasMaxLength(1000);
            builder.Property(x => x.DailyPerDiemRate).HasColumnType("decimal(18,2)");
            builder.Property(x => x.PerDiemAmount).HasColumnType("decimal(18,2)");
            builder.Property(x => x.AdvanceAmount).HasColumnType("decimal(18,2)");
            builder.Property(x => x.SettlementNet).HasColumnType("decimal(18,2)");
            builder.Property(x => x.Currency).IsRequired().HasMaxLength(10);
            builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.Resolution).HasMaxLength(2000);
            builder.Property(x => x.AdvanceReference).HasMaxLength(100);
            builder.Property(x => x.SettlementReference).HasMaxLength(100);

            builder.HasOne<Employee>().WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<TripBudget>().WithMany().HasForeignKey(x => x.TripBudgetId).OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(x => x.Expenses).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(x => new { x.EmployeeId, x.Status });
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.TripBudgetId);
            builder.HasIndex(x => new { x.TenantId, x.TripNumber }).IsUnique();
        }
    }

    public class TripExpenseConfiguration : IEntityTypeConfiguration<TripExpense>
    {
        public void Configure(EntityTypeBuilder<TripExpense> builder)
        {
            builder.ToTable("hrmsTripExpense", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Category).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            builder.Property(x => x.Currency).IsRequired().HasMaxLength(10);

            builder.HasOne<TripRequest>().WithMany(t => t.Expenses).HasForeignKey(x => x.TripRequestId).OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(x => x.TripRequestId);
        }
    }
}
