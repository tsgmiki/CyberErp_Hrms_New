using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class ProfileChangeRequestConfiguration : IEntityTypeConfiguration<ProfileChangeRequest>
    {
        public void Configure(EntityTypeBuilder<ProfileChangeRequest> builder)
        {
            builder.ToTable("ProfileChangeRequest", "Hrms");
            builder.HasKey(r => r.Id);

            builder.Property(r => r.FieldKey).IsRequired().HasMaxLength(60);
            builder.Property(r => r.FieldLabel).IsRequired().HasMaxLength(120);
            builder.Property(r => r.Kind).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(r => r.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(r => r.CurrentValue).HasMaxLength(2000);
            builder.Property(r => r.RequestedValue).IsRequired().HasMaxLength(2000);
            builder.Property(r => r.Reason).HasMaxLength(2000);
            builder.Property(r => r.Resolution).HasMaxLength(2000);
            builder.Property(r => r.ResolvedBy).HasMaxLength(200);

            builder.HasOne<Employee>().WithMany().HasForeignKey(r => r.EmployeeId).OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(r => r.EmployeeId);
            builder.HasIndex(r => r.Status);
        }
    }
}
