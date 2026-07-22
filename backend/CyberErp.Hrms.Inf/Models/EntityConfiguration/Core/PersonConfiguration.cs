using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class PersonConfiguration : IEntityTypeConfiguration<Person>
    {
        public void Configure(EntityTypeBuilder<Person> builder)
        {
            builder.ToTable("CorePerson", "Core");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
            builder.Property(p => p.FirstNameA).HasMaxLength(100);
            builder.Property(p => p.FatherName).HasMaxLength(100);
            builder.Property(p => p.FatherNameA).HasMaxLength(100);
            builder.Property(p => p.GrandFatherName).IsRequired().HasMaxLength(100);
            builder.Property(p => p.GrandFatherNameA).HasMaxLength(100);
            builder.Property(p => p.Gender).HasConversion<string>().HasMaxLength(20).IsRequired();
            // Stored as the numeric enum value — the column is named MaritalStatusId by spec.
            builder.Property(p => p.MaritalStatusId).HasConversion<int>().IsRequired();
            builder.Property(p => p.PhoneNumber).HasMaxLength(50);
            builder.Property(p => p.LocationName).HasMaxLength(500);
            builder.Property(p => p.IsDeleted).IsRequired().HasDefaultValue(false);

            builder.Ignore(p => p.FullName);

            builder.HasIndex(p => new { p.FirstName, p.FatherName, p.GrandFatherName });
        }
    }
}
