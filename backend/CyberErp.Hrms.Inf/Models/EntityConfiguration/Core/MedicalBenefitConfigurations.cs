using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class MedicalProviderConfiguration : IEntityTypeConfiguration<MedicalProvider>
    {
        public void Configure(EntityTypeBuilder<MedicalProvider> builder)
        {
            builder.ToTable("hrmsMedicalProvider", "dbo");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
            builder.Property(p => p.ProviderType).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(p => p.Specialization).HasMaxLength(300);
            builder.Property(p => p.PhoneNumber).HasMaxLength(50);
            builder.Property(p => p.Email).HasMaxLength(150);
            builder.Property(p => p.Address).HasMaxLength(500);
            builder.Property(p => p.IsActive).HasDefaultValue(true);

            builder.HasIndex(p => new { p.TenantId, p.Name }).IsUnique();
        }
    }

    public class MedicalPlanConfiguration : IEntityTypeConfiguration<MedicalPlan>
    {
        public void Configure(EntityTypeBuilder<MedicalPlan> builder)
        {
            builder.ToTable("hrmsMedicalPlan", "dbo");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
            builder.Property(p => p.Description).HasMaxLength(1000);
            builder.Property(p => p.AnnualCoverageLimit).HasColumnType("decimal(18,2)");
            builder.Property(p => p.CoveragePercent).HasColumnType("decimal(18,2)");
            builder.Property(p => p.CoversDependents).HasDefaultValue(true);
            builder.Property(p => p.IsActive).HasDefaultValue(true);

            // Optional premium link — no FK cascade coupling to the benefit plan.
            builder.HasIndex(p => new { p.TenantId, p.Name }).IsUnique();
        }
    }

    public class MedicalClaimConfiguration : IEntityTypeConfiguration<MedicalClaim>
    {
        public void Configure(EntityTypeBuilder<MedicalClaim> builder)
        {
            builder.ToTable("hrmsMedicalClaim", "dbo");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.ClaimNumber).IsRequired().HasMaxLength(30);
            builder.Property(c => c.BeneficiaryCategory).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(c => c.Source).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(c => c.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(c => c.ClaimedAmount).HasColumnType("decimal(18,2)");
            builder.Property(c => c.ApprovedAmount).HasColumnType("decimal(18,2)");
            builder.Property(c => c.Description).IsRequired().HasMaxLength(2000);
            builder.Property(c => c.Diagnosis).HasMaxLength(1000);
            builder.Property(c => c.Resolution).HasMaxLength(2000);
            builder.Property(c => c.PaymentReference).HasMaxLength(100);

            builder.HasOne<Employee>().WithMany().HasForeignKey(c => c.EmployeeId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<MedicalEnrollment>().WithMany().HasForeignKey(c => c.MedicalEnrollmentId).OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(c => c.Attachments).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(c => c.EmployeeId);
            builder.HasIndex(c => c.Status);
            builder.HasIndex(c => new { c.MedicalBeneficiaryId, c.Status });
            // Per-tenant unique — the number sequence restarts per tenant.
            builder.HasIndex(c => new { c.TenantId, c.ClaimNumber }).IsUnique();
        }
    }

    public class MedicalClaimAttachmentConfiguration : IEntityTypeConfiguration<MedicalClaimAttachment>
    {
        public void Configure(EntityTypeBuilder<MedicalClaimAttachment> builder)
        {
            builder.ToTable("hrmsMedicalClaimAttachment", "dbo");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.FileName).IsRequired().HasMaxLength(300);
            builder.Property(a => a.ContentType).IsRequired().HasMaxLength(150);
            builder.Property(a => a.Content).IsRequired();

            builder.HasOne<MedicalClaim>()
                .WithMany(c => c.Attachments)
                .HasForeignKey(a => a.MedicalClaimId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(a => a.MedicalClaimId);
        }
    }

    public class MedicalEnrollmentConfiguration : IEntityTypeConfiguration<MedicalEnrollment>
    {
        public void Configure(EntityTypeBuilder<MedicalEnrollment> builder)
        {
            builder.ToTable("hrmsMedicalEnrollment", "dbo");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(e => e.Remark).HasMaxLength(500);

            builder.HasOne<Employee>().WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<MedicalPlan>().WithMany().HasForeignKey(e => e.MedicalPlanId).OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(e => e.Beneficiaries).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(e => e.EmployeeId);
            builder.HasIndex(e => e.MedicalPlanId);
        }
    }

    public class MedicalBeneficiaryConfiguration : IEntityTypeConfiguration<MedicalBeneficiary>
    {
        public void Configure(EntityTypeBuilder<MedicalBeneficiary> builder)
        {
            builder.ToTable("hrmsMedicalBeneficiary", "dbo");
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Category).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(b => b.FullName).IsRequired().HasMaxLength(200);
            builder.Property(b => b.Relationship).HasMaxLength(50);

            builder.HasOne<MedicalEnrollment>()
                .WithMany(e => e.Beneficiaries)
                .HasForeignKey(b => b.MedicalEnrollmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(b => b.MedicalEnrollmentId);
        }
    }

    public class MedicalServiceContractConfiguration : IEntityTypeConfiguration<MedicalServiceContract>
    {
        public void Configure(EntityTypeBuilder<MedicalServiceContract> builder)
        {
            builder.ToTable("hrmsMedicalServiceContract", "dbo");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.ContractNumber).HasMaxLength(50);
            builder.Property(c => c.Terms).HasMaxLength(2000);
            builder.Property(c => c.CreditLimit).HasColumnType("decimal(18,2)");
            builder.Property(c => c.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(c => c.Notes).HasMaxLength(1000);

            builder.HasOne<MedicalProvider>()
                .WithMany()
                .HasForeignKey(c => c.MedicalProviderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(c => c.MedicalProviderId);
            builder.HasIndex(c => c.Status);
        }
    }
}
