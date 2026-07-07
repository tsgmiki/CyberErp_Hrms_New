using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class EmployeeTerminationConfiguration : IEntityTypeConfiguration<EmployeeTermination>
    {
        public void Configure(EntityTypeBuilder<EmployeeTermination> builder)
        {
            builder.ToTable("hrms_EmployeeTermination", "Core");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.TerminationType).IsRequired().HasConversion<string>().HasMaxLength(30);
            builder.Property(t => t.Status).IsRequired().HasConversion<string>().HasMaxLength(30);
            builder.Property(t => t.Reason).IsRequired().HasMaxLength(1000);
            builder.Property(t => t.Remarks).HasMaxLength(2000);

            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(t => t.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(t => t.Clearances)
                .WithOne()
                .HasForeignKey(c => c.TerminationId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(t => t.Clearances).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(t => t.EmployeeId);
            builder.HasIndex(t => t.Status);
        }
    }

    public class TerminationClearanceConfiguration : IEntityTypeConfiguration<TerminationClearance>
    {
        public void Configure(EntityTypeBuilder<TerminationClearance> builder)
        {
            builder.ToTable("hrms_TerminationClearance", "Core");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Department).IsRequired().HasMaxLength(100);
            builder.Property(c => c.Description).IsRequired().HasMaxLength(500);
            builder.Property(c => c.Status).IsRequired().HasConversion<string>().HasMaxLength(30);
            builder.Property(c => c.Note).HasMaxLength(1000);
            builder.Property(c => c.ClearedBy).HasMaxLength(200);

            // Deleting a configured department must not break existing checklist rows — they
            // fall back to the built-in "open" behaviour (no approver restriction).
            builder.HasOne<ClearanceDepartment>()
                .WithMany()
                .HasForeignKey(c => c.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(c => c.TerminationId);
            builder.HasIndex(c => c.DepartmentId);
        }
    }

    public class ClearanceDepartmentConfiguration : IEntityTypeConfiguration<ClearanceDepartment>
    {
        public void Configure(EntityTypeBuilder<ClearanceDepartment> builder)
        {
            builder.ToTable("hrms_ClearanceDepartment", "Core");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Name).IsRequired().HasMaxLength(100);
            builder.Property(d => d.Description).IsRequired().HasMaxLength(500);

            builder.HasMany(d => d.Approvers)
                .WithOne()
                .HasForeignKey(a => a.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(d => d.Approvers).UsePropertyAccessMode(PropertyAccessMode.Field);

            // Uniqueness (per tenant, by name) is enforced in the save handler.
            builder.HasIndex(d => new { d.TenantId, d.Name });
        }
    }

    public class ClearanceDepartmentApproverConfiguration : IEntityTypeConfiguration<ClearanceDepartmentApprover>
    {
        public void Configure(EntityTypeBuilder<ClearanceDepartmentApprover> builder)
        {
            builder.ToTable("hrms_ClearanceDepartmentApprover", "Core");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.ApproverType).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(a => a.DisplayName).IsRequired().HasMaxLength(300);

            builder.HasIndex(a => a.DepartmentId);
            builder.HasIndex(a => new { a.ApproverType, a.ApproverId });
        }
    }
}
