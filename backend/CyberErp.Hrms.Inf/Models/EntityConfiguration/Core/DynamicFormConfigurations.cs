using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class DynamicFormConfiguration : IEntityTypeConfiguration<DynamicForm>
    {
        public void Configure(EntityTypeBuilder<DynamicForm> builder)
        {
            builder.ToTable("hrmsDynamicForm", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Module).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Label).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.Property(x => x.Icon).HasMaxLength(50);

            builder.HasMany(x => x.Fields)
                .WithOne()
                .HasForeignKey(f => f.DynamicFormId)
                .OnDelete(DeleteBehavior.Cascade);

            // Form names are unique per (tenant, module).
            builder.HasIndex(x => new { x.TenantId, x.Module, x.Name }).IsUnique();
        }
    }

    public class DynamicFormFieldConfiguration : IEntityTypeConfiguration<DynamicFormField>
    {
        public void Configure(EntityTypeBuilder<DynamicFormField> builder)
        {
            builder.ToTable("hrmsDynamicFormField", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Label).IsRequired().HasMaxLength(200);
            builder.Property(x => x.DataType).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.Options).HasMaxLength(2000);

            builder.HasIndex(x => new { x.DynamicFormId, x.Name }).IsUnique();
        }
    }

    public class DynamicFormRecordConfiguration : IEntityTypeConfiguration<DynamicFormRecord>
    {
        public void Configure(EntityTypeBuilder<DynamicFormRecord> builder)
        {
            builder.ToTable("hrmsDynamicFormRecord", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.OwnerType).IsRequired().HasMaxLength(30);
            builder.Property(x => x.Data).IsRequired();   // nvarchar(max) JSON document

            // Definition FK is Restrict — a form with records can't be dropped (deactivate instead).
            builder.HasOne<DynamicForm>()
                .WithMany()
                .HasForeignKey(x => x.DynamicFormId)
                .OnDelete(DeleteBehavior.Restrict);

            // The hot path: page a form's records for one owner, newest first. CreatedAt is in the key
            // so the seek AND the ordered pagination are index-supported (no sort operator).
            builder.HasIndex(x => new { x.DynamicFormId, x.OwnerType, x.OwnerId, x.CreatedAt });
        }
    }
}
