using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class LeaveTypeConfiguration : IEntityTypeConfiguration<LeaveType>
    {
        public void Configure(EntityTypeBuilder<LeaveType> builder)
        {
            builder.ToTable("hrmsLeaveType", "dbo");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.NameA).HasMaxLength(200);
            builder.Property(x => x.Description).HasMaxLength(1000);

            builder.Property(x => x.GenderEligibility).HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.AccrualMethod).HasConversion<string>().HasMaxLength(20);

            builder.Property(x => x.DefaultAnnualEntitlement).HasPrecision(6, 2);
            builder.Property(x => x.CarryForwardMaxDays).HasPrecision(6, 2);

            builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        }
    }
}
