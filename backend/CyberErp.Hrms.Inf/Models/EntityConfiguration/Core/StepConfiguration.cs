using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    /// <summary>Maps <see cref="Step"/> onto the <c>lupStep</c> lookup table.</summary>
    public class StepConfiguration : IEntityTypeConfiguration<Step>
    {
        public void Configure(EntityTypeBuilder<Step> builder)
        {
            builder.ToTable("lupStep", "Core");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
            builder.Property(s => s.Code).IsRequired().HasMaxLength(50);

            builder.HasIndex(s => new { s.TenantId, s.Code }).IsUnique();
        }
    }
}
