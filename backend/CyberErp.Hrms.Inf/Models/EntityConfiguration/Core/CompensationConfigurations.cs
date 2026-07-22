using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class AllowanceTypeConfiguration : IEntityTypeConfiguration<AllowanceType>
    {
        public void Configure(EntityTypeBuilder<AllowanceType> builder)
        {
            builder.ToTable("hrmsAllowanceType", "dbo");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Name).IsRequired().HasMaxLength(150);
            builder.Property(a => a.Code).HasMaxLength(50);
            builder.Property(a => a.CalcMethod).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(a => a.DefaultRate).HasColumnType("decimal(18,2)");
            builder.Property(a => a.IsTaxable).HasDefaultValue(true);
            builder.Property(a => a.IsActive).HasDefaultValue(true);

            builder.HasIndex(a => new { a.TenantId, a.Name }).IsUnique();
        }
    }

    public class CompensationRequestConfiguration : IEntityTypeConfiguration<CompensationRequest>
    {
        public void Configure(EntityTypeBuilder<CompensationRequest> builder)
        {
            builder.ToTable("hrmsCompensationRequest", "dbo");
            builder.HasKey(r => r.Id);

            builder.Property(r => r.RequestType).IsRequired().HasConversion<string>().HasMaxLength(30);
            builder.Property(r => r.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(r => r.Subject).IsRequired().HasMaxLength(300);
            builder.Property(r => r.Details).IsRequired().HasMaxLength(4000);
            builder.Property(r => r.ReferencePeriod).HasMaxLength(50);
            builder.Property(r => r.DisputedAmount).HasColumnType("decimal(18,2)");
            builder.Property(r => r.Resolution).HasMaxLength(4000);

            builder.HasOne<Employee>().WithMany().HasForeignKey(r => r.EmployeeId).OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(r => r.EmployeeId);
            builder.HasIndex(r => r.Status);
        }
    }

    public class BenefitPlanConfiguration : IEntityTypeConfiguration<BenefitPlan>
    {
        public void Configure(EntityTypeBuilder<BenefitPlan> builder)
        {
            builder.ToTable("hrmsBenefitPlan", "dbo");
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Name).IsRequired().HasMaxLength(200);
            builder.Property(b => b.Category).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(b => b.Description).HasMaxLength(1000);
            builder.Property(b => b.EmployeeContributionMethod).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(b => b.EmployerContributionMethod).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(b => b.EmployeeContributionRate).HasColumnType("decimal(18,2)");
            builder.Property(b => b.EmployerContributionRate).HasColumnType("decimal(18,2)");
            builder.Property(b => b.IsActive).HasDefaultValue(true);

            builder.HasIndex(b => new { b.TenantId, b.Name }).IsUnique();
        }
    }

    public class EmployeeBenefitEnrollmentConfiguration : IEntityTypeConfiguration<EmployeeBenefitEnrollment>
    {
        public void Configure(EntityTypeBuilder<EmployeeBenefitEnrollment> builder)
        {
            builder.ToTable("hrmsEmployeeBenefitEnrollment", "dbo");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(e => e.ElectedEmployeeContribution).HasColumnType("decimal(18,2)");
            builder.Property(e => e.Remark).HasMaxLength(500);

            builder.HasOne<Employee>().WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<BenefitPlan>().WithMany().HasForeignKey(e => e.BenefitPlanId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(e => e.EmployeeId);
            builder.HasIndex(e => e.BenefitPlanId);
        }
    }

    public class TaxBracketConfiguration : IEntityTypeConfiguration<TaxBracket>
    {
        public void Configure(EntityTypeBuilder<TaxBracket> builder)
        {
            builder.ToTable("hrmsTaxBracket", "dbo");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.LowerBound).HasColumnType("decimal(18,2)");
            builder.Property(t => t.UpperBound).HasColumnType("decimal(18,2)");
            builder.Property(t => t.RatePercent).HasColumnType("decimal(18,2)");
        }
    }

    public class SalaryRevisionConfiguration : IEntityTypeConfiguration<SalaryRevision>
    {
        public void Configure(EntityTypeBuilder<SalaryRevision> builder)
        {
            builder.ToTable("hrmsSalaryRevision", "dbo");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Name).IsRequired().HasMaxLength(200);
            builder.Property(r => r.RevisionType).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(r => r.Basis).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(r => r.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(r => r.Rate).HasColumnType("decimal(18,2)");
            builder.Property(r => r.Notes).HasMaxLength(1000);

            builder.Navigation(r => r.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasIndex(r => r.Status);
        }
    }

    public class SalaryRevisionLineConfiguration : IEntityTypeConfiguration<SalaryRevisionLine>
    {
        public void Configure(EntityTypeBuilder<SalaryRevisionLine> builder)
        {
            builder.ToTable("hrmsSalaryRevisionLine", "dbo");

            builder.HasKey(l => l.Id);

            builder.Property(l => l.CurrentSalary).HasColumnType("decimal(18,2)");
            builder.Property(l => l.ProposedSalary).HasColumnType("decimal(18,2)");

            builder.HasOne<SalaryRevision>()
                .WithMany(r => r.Lines)
                .HasForeignKey(l => l.SalaryRevisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(l => l.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(l => l.SalaryRevisionId);
            builder.HasIndex(l => l.EmployeeId);
        }
    }

    public class EmployeeAllowanceConfiguration : IEntityTypeConfiguration<EmployeeAllowance>
    {
        public void Configure(EntityTypeBuilder<EmployeeAllowance> builder)
        {
            builder.ToTable("hrmsEmployeeAllowance", "dbo");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Value).IsRequired().HasColumnType("decimal(18,2)");
            builder.Property(e => e.Remark).HasMaxLength(500);

            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Restrict: a type in use cannot be deleted out from under an assignment.
            builder.HasOne<AllowanceType>()
                .WithMany()
                .HasForeignKey(e => e.AllowanceTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Per-employee history read is the hot path (compensation summary).
            builder.HasIndex(e => e.EmployeeId);
            builder.HasIndex(e => e.AllowanceTypeId);
        }
    }
}
