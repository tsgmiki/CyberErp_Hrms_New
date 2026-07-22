using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class LoanTypeConfiguration : IEntityTypeConfiguration<LoanType>
    {
        public void Configure(EntityTypeBuilder<LoanType> builder)
        {
            builder.ToTable("hrmsLoanType", "dbo");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name).IsRequired().HasMaxLength(150);
            builder.Property(t => t.Description).HasMaxLength(1000);
            builder.Property(t => t.MaxAmount).HasColumnType("decimal(18,2)");
            builder.Property(t => t.MaxSalaryMultiple).HasColumnType("decimal(9,2)");
            builder.Property(t => t.InterestRatePct).HasColumnType("decimal(9,4)");
            builder.Property(t => t.IsActive).HasDefaultValue(true);

            builder.HasIndex(t => new { t.TenantId, t.Name }).IsUnique();
        }
    }

    public class LoanConfiguration : IEntityTypeConfiguration<Loan>
    {
        public void Configure(EntityTypeBuilder<Loan> builder)
        {
            builder.ToTable("hrmsLoan", "dbo");
            builder.HasKey(l => l.Id);

            builder.Property(l => l.LoanNumber).IsRequired().HasMaxLength(30);
            builder.Property(l => l.PrincipalAmount).HasColumnType("decimal(18,2)");
            builder.Property(l => l.InterestRatePct).HasColumnType("decimal(9,4)");
            builder.Property(l => l.MonthlyInstallment).HasColumnType("decimal(18,2)");
            builder.Property(l => l.TotalInterest).HasColumnType("decimal(18,2)");
            builder.Property(l => l.TotalRepayable).HasColumnType("decimal(18,2)");
            builder.Property(l => l.Purpose).HasMaxLength(1000);
            builder.Property(l => l.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(l => l.Resolution).HasMaxLength(2000);
            builder.Property(l => l.DisbursementReference).HasMaxLength(100);

            builder.HasOne<Employee>().WithMany().HasForeignKey(l => l.EmployeeId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<LoanType>().WithMany().HasForeignKey(l => l.LoanTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(l => l.Guarantors).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Navigation(l => l.Schedule).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(l => new { l.EmployeeId, l.Status });
            builder.HasIndex(l => l.Status);
            builder.HasIndex(l => new { l.TenantId, l.LoanNumber }).IsUnique();
        }
    }

    public class LoanGuarantorConfiguration : IEntityTypeConfiguration<LoanGuarantor>
    {
        public void Configure(EntityTypeBuilder<LoanGuarantor> builder)
        {
            builder.ToTable("hrmsLoanGuarantor", "dbo");
            builder.HasKey(g => g.Id);

            builder.Property(g => g.FullName).IsRequired().HasMaxLength(200);
            builder.Property(g => g.IdentificationNumber).HasMaxLength(100);
            builder.Property(g => g.Relationship).HasMaxLength(100);
            builder.Property(g => g.PhoneNumber).HasMaxLength(50);
            builder.Property(g => g.GuaranteedAmount).HasColumnType("decimal(18,2)");

            builder.HasOne<Loan>().WithMany(l => l.Guarantors).HasForeignKey(g => g.LoanId).OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(g => g.LoanId);
        }
    }

    public class LoanRepaymentScheduleLineConfiguration : IEntityTypeConfiguration<LoanRepaymentScheduleLine>
    {
        public void Configure(EntityTypeBuilder<LoanRepaymentScheduleLine> builder)
        {
            builder.ToTable("hrmsLoanRepaymentSchedule", "dbo");
            builder.HasKey(s => s.Id);

            builder.Property(s => s.PrincipalPortion).HasColumnType("decimal(18,2)");
            builder.Property(s => s.InterestPortion).HasColumnType("decimal(18,2)");
            builder.Property(s => s.Amount).HasColumnType("decimal(18,2)");
            builder.Property(s => s.Status).IsRequired().HasConversion<string>().HasMaxLength(20);

            builder.HasOne<Loan>().WithMany(l => l.Schedule).HasForeignKey(s => s.LoanId).OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(s => new { s.LoanId, s.InstallmentNo });
            builder.HasIndex(s => new { s.Status, s.DueDate });
        }
    }
}
