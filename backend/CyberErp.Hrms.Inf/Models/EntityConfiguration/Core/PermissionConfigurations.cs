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

            builder.Property(m => m.Name).IsRequired().HasMaxLength(200);
            builder.Property(m => m.Icon).HasMaxLength(200);
            builder.Property(m => m.SortOrder).HasDefaultValue(0);

            builder.HasOne(m => m.Subsystem)
                .WithMany()
                .HasForeignKey(m => m.SubsystemId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(m => m.Subsystem).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(m => m.SubsystemId);
            // No Operations collection any more — the menu tree lives inside Core.Operation itself.
        }
    }

    /// <summary>
    /// Master subsystem list (Core.Subsystem) — the ERP-wide table the HRMS now maps.
    /// Modules reference a subsystem via the SubsystemId FK.
    /// </summary>
    public class SubsystemConfiguration : IEntityTypeConfiguration<Subsystem>
    {
        public void Configure(EntityTypeBuilder<Subsystem> builder)
        {
            builder.ToTable("Subsystem", "Core");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
            builder.Property(s => s.Code).IsRequired().HasMaxLength(50);
            builder.Property(s => s.SortOrder).HasDefaultValue(0);
            // Where the subsystem's app lives — the Home portal's launcher tiles deep-link here.
            builder.Property(s => s.Url).HasMaxLength(400);

            // SRMS platform alignment (2026-08-14, logic.md §12.13). The six columns SRMS carries
            // that CERP lacked; defaults keep existing rows and new inserts valid without a value.
            builder.Property(s => s.Abbreviation).HasMaxLength(50);
            builder.Property(s => s.Icon).HasMaxLength(100);
            builder.Property(s => s.Description).IsRequired().HasMaxLength(500).HasDefaultValue(string.Empty);
            builder.Property(s => s.DisplayOrder).IsRequired().HasDefaultValue(0);
            builder.Property(s => s.IsActive).IsRequired().HasDefaultValue(true);
            builder.Property(s => s.LandingPath).IsRequired().HasMaxLength(250).HasDefaultValue(string.Empty);

            builder.HasIndex(s => new { s.TenantId, s.Name }).IsUnique();
        }
    }

    public class OperationConfiguration : IEntityTypeConfiguration<Operation>
    {
        public void Configure(EntityTypeBuilder<Operation> builder)
        {
            builder.ToTable("Operation", "Core");

            // TenantId is GONE — the per-tenant copy is Core.TenantOperation.


            builder.Ignore(o => o.TenantId);



            builder.HasKey(o => o.Id);

            // Lengths narrowed to the SRMS caps; the longest value in any of these is 25 characters.
            builder.Property(o => o.Name).IsRequired().HasMaxLength(100);
            builder.Property(o => o.Link).IsRequired().HasMaxLength(200);
            builder.Property(o => o.Filter).IsRequired().HasMaxLength(200);
            builder.Property(o => o.Icon).IsRequired().HasMaxLength(100);
            builder.Property(o => o.DisplayOrder).HasDefaultValue(0);   // was SortOrder
            builder.Property(o => o.IsActive).IsRequired().HasDefaultValue(true);

            // ModuleId is the PARENT LINK — a self-reference, not a foreign key to Core.Module.
            // NoAction is not a preference: SQL Server rejects a cascading self-referencing foreign
            // key outright (it cannot prove the chain terminates). Deleting a parent therefore has to
            // clear its children first, which DeleteOperationHandler does.
            builder.HasOne(o => o.Parent)
                .WithMany(o => o.Children)
                .HasForeignKey(o => o.ModuleId)
                .OnDelete(DeleteBehavior.NoAction);
            builder.Navigation(o => o.Parent).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Navigation(o => o.Children).UsePropertyAccessMode(PropertyAccessMode.Field);

            // Denormalised subsystem. Restrict, not Cascade: deleting a subsystem must not silently
            // take the menu with it, and Module already cascades.
            builder.HasOne<Subsystem>()
                .WithMany()
                .HasForeignKey(o => o.SubSystemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(o => o.ModuleId);
            builder.HasIndex(o => o.SubSystemId);
            builder.HasIndex(o => new { o.SubSystemId, o.ModuleId, o.DisplayOrder });
        }
    }

}
