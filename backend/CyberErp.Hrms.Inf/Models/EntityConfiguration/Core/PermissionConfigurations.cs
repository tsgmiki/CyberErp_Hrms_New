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

            // ModuleId is a FOREIGN KEY to Core.Module again (2026-08-15) — SRMS was corrected, and
            // this follows it. The constraint NAME is SRMS's, not EF's convention.
            builder.HasOne(o => o.Module)
                .WithMany()
                .HasForeignKey(o => o.ModuleId)
                .HasConstraintName("FK_NavigationOperation_Module_ModuleId")
                .OnDelete(DeleteBehavior.NoAction);
            builder.Navigation(o => o.Module).UsePropertyAccessMode(PropertyAccessMode.Field);

            // ⚠️ The subsystem FK is CASCADE and is called FK_Operation_Module_ModuleId — both
            // copied from SRMS verbatim, per the "identical structure" requirement. The name is a
            // MISNOMER there (a leftover from a rename; it constrains SubSystemId, not ModuleId) and
            // the cascade means deleting a subsystem takes its whole menu with it, which CERP
            // previously refused with Restrict. Kept identical deliberately — do not "fix" either
            // without changing SRMS first, or the databases diverge again.
            builder.HasOne<Subsystem>()
                .WithMany()
                .HasForeignKey(o => o.SubSystemId)
                .HasConstraintName("FK_Operation_Module_ModuleId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(o => o.ModuleId);
            builder.HasIndex(o => o.SubSystemId);
            builder.HasIndex(o => new { o.SubSystemId, o.ModuleId, o.DisplayOrder });
        }
    }

}
