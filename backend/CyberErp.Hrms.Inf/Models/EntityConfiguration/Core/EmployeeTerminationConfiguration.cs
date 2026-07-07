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

            builder.HasIndex(c => c.TerminationId);
        }
    }
}
