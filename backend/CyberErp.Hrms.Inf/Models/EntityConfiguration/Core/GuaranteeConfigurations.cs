using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    // ===== §3.12 Employee Guarantee Commitment Management (HC305–HC307) =====

    public class EmployeeGuaranteeConfiguration : IEntityTypeConfiguration<EmployeeGuarantee>
    {
        public void Configure(EntityTypeBuilder<EmployeeGuarantee> builder)
        {
            builder.ToTable("hrmsEmployeeGuarantee", "dbo");
            builder.HasKey(x => x.Id);

            // Lookup-driven (global "GuaranteeType" category value, stored by name).
            builder.Property(x => x.Type).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.ExternalOrganization).IsRequired().HasMaxLength(200);
            builder.Property(x => x.BeneficiaryName).IsRequired().HasMaxLength(200);
            builder.Property(x => x.BeneficiaryRelationship).HasMaxLength(100);
            builder.Property(x => x.ReferenceNumber).HasMaxLength(100);
            builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            builder.Property(x => x.Remarks).HasMaxLength(2000);
            builder.Property(x => x.ReleaseNote).HasMaxLength(1000);

            builder.HasOne<Employee>().WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.TenantId, x.EmployeeId });
            builder.HasIndex(x => new { x.TenantId, x.Status });
            // The expiring-soon dashboard chip scans by end date.
            builder.HasIndex(x => new { x.TenantId, x.EndDate });
        }
    }
}
