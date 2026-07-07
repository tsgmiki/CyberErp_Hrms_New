using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    /// <summary>
    /// Maps the pre-existing permission model (Core.Module / Core.Operation / Core.RolePermission,
    /// created by the template's Initial migration). These entities entered the EF model when
    /// Role gained its RolePermissions navigation; explicit FKs keep EF from inventing shadow
    /// columns (the DB's legacy RoleId1 / UserId1 columns are intentionally left unmapped).
    /// </summary>
    public class ModuleConfiguration : IEntityTypeConfiguration<Module>
    {
        public void Configure(EntityTypeBuilder<Module> builder)
        {
            builder.ToTable("Module", "Core");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.SubSystem).IsRequired().HasMaxLength(200);
            builder.Property(m => m.Name).IsRequired().HasMaxLength(200);
            builder.Property(m => m.Icon).HasMaxLength(200);

            builder.Navigation(m => m.Operations).UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }

    public class OperationConfiguration : IEntityTypeConfiguration<Operation>
    {
        public void Configure(EntityTypeBuilder<Operation> builder)
        {
            builder.ToTable("Operation", "Core");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Name).IsRequired().HasMaxLength(200);
            builder.Property(o => o.Link).IsRequired().HasMaxLength(400);
            builder.Property(o => o.Filter).IsRequired().HasMaxLength(400);
            builder.Property(o => o.Icon).IsRequired().HasMaxLength(200);

            builder.HasOne(o => o.Module)
                .WithMany(m => m.Operations)
                .HasForeignKey(o => o.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(o => o.Module).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(o => o.ModuleId);
        }
    }

    public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            builder.ToTable("RolePermission", "Core");

            builder.HasKey(rp => rp.Id);

            // Real FK columns — no EF-invented shadow keys.
            builder.HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(rp => rp.Operation)
                .WithMany()
                .HasForeignKey(rp => rp.OperationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Navigation(rp => rp.Role).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Navigation(rp => rp.Operation).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(rp => rp.RoleId);
            builder.HasIndex(rp => rp.OperationId);
        }
    }
}
