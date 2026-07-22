using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class InsurancePolicyConfiguration : IEntityTypeConfiguration<InsurancePolicy>
    {
        public void Configure(EntityTypeBuilder<InsurancePolicy> builder)
        {
            builder.ToTable("hrmsInsurancePolicy", "dbo");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.PolicyNumber).IsRequired().HasMaxLength(50);
            builder.Property(p => p.InsurerName).IsRequired().HasMaxLength(200);
            builder.Property(p => p.InsuranceType).IsRequired().HasConversion<string>().HasMaxLength(30);
            builder.Property(p => p.Coverage).HasMaxLength(1000);
            builder.Property(p => p.CoverageAmount).HasColumnType("decimal(18,2)");
            builder.Property(p => p.AnnualPremium).HasColumnType("decimal(18,2)");
            builder.Property(p => p.PremiumFrequency).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(p => p.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(p => p.Notes).HasMaxLength(1000);

            builder.Navigation(p => p.PremiumSchedule).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(p => p.Status);
            // Per-tenant unique — policy numbers are tenant-scoped.
            builder.HasIndex(p => new { p.TenantId, p.PolicyNumber }).IsUnique();
        }
    }

    public class InsurancePremiumScheduleConfiguration : IEntityTypeConfiguration<InsurancePremiumSchedule>
    {
        public void Configure(EntityTypeBuilder<InsurancePremiumSchedule> builder)
        {
            builder.ToTable("hrmsInsurancePremiumSchedule", "dbo");
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Amount).HasColumnType("decimal(18,2)");
            builder.Property(s => s.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(s => s.PaymentReference).HasMaxLength(100);

            builder.HasOne<InsurancePolicy>()
                .WithMany(p => p.PremiumSchedule)
                .HasForeignKey(s => s.InsurancePolicyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(s => new { s.InsurancePolicyId, s.Installment });
            builder.HasIndex(s => new { s.Status, s.DueDate });
        }
    }

    public class InsuranceClaimConfiguration : IEntityTypeConfiguration<InsuranceClaim>
    {
        public void Configure(EntityTypeBuilder<InsuranceClaim> builder)
        {
            builder.ToTable("hrmsInsuranceClaim", "dbo");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.ClaimNumber).IsRequired().HasMaxLength(30);
            builder.Property(c => c.ClaimType).IsRequired().HasMaxLength(100);
            builder.Property(c => c.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(c => c.ClaimedAmount).HasColumnType("decimal(18,2)");
            builder.Property(c => c.ApprovedAmount).HasColumnType("decimal(18,2)");
            builder.Property(c => c.Description).IsRequired().HasMaxLength(2000);
            builder.Property(c => c.Resolution).HasMaxLength(2000);
            builder.Property(c => c.PaymentReference).HasMaxLength(100);

            builder.HasOne<Employee>().WithMany().HasForeignKey(c => c.EmployeeId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<InsurancePolicy>().WithMany().HasForeignKey(c => c.InsurancePolicyId).OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(c => c.Attachments).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(c => c.EmployeeId);
            builder.HasIndex(c => c.Status);
            builder.HasIndex(c => c.InsurancePolicyId);
            // Per-tenant unique — the number sequence restarts per tenant.
            builder.HasIndex(c => new { c.TenantId, c.ClaimNumber }).IsUnique();
        }
    }

    public class InsuranceClaimAttachmentConfiguration : IEntityTypeConfiguration<InsuranceClaimAttachment>
    {
        public void Configure(EntityTypeBuilder<InsuranceClaimAttachment> builder)
        {
            builder.ToTable("hrmsInsuranceClaimAttachment", "dbo");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.FileName).IsRequired().HasMaxLength(300);
            builder.Property(a => a.ContentType).IsRequired().HasMaxLength(150);
            builder.Property(a => a.Content).IsRequired();

            builder.HasOne<InsuranceClaim>()
                .WithMany(c => c.Attachments)
                .HasForeignKey(a => a.InsuranceClaimId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(a => a.InsuranceClaimId);
        }
    }
}
