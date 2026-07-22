using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class LookupCategoryConfiguration : IEntityTypeConfiguration<LookupCategory>
    {
        public void Configure(EntityTypeBuilder<LookupCategory> builder)
        {
            builder.ToTable("LookUpCategory", "Core");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
            // Lookups are GLOBAL (shared, TenantId = '') → the code is unique system-wide.
            builder.HasIndex(x => x.Code).IsUnique();

            builder.HasMany(x => x.Items)
                .WithOne()
                .HasForeignKey(i => i.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(x => x.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }

    public class LookupCategoryListConfiguration : IEntityTypeConfiguration<LookupCategoryList>
    {
        public void Configure(EntityTypeBuilder<LookupCategoryList> builder)
        {
            builder.ToTable("LookUpCategoryList", "Core");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
            builder.HasIndex(x => new { x.CategoryId, x.Code }).IsUnique();
        }
    }
}
