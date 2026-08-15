using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    /*
     * Tenant-scoped authorization (SRMS phase 2).
     *
     * None of these is marked [MultiTenant]. They are the tables that DEFINE tenant scoping, so they
     * cannot themselves be filtered by the ambient tenant without a chicken-and-egg problem: sign-in
     * has to read a user's tenant memberships BEFORE a tenant context exists. Scoping is explicit,
     * through TenantId, a uniqueidentifier since the 2026-08-14 re-key. It doubles as the foreign key
     * to Core.Tenant, added at the DATABASE level because EF cannot model a relationship on a
     * value-converted property. That is how SRMS models it, and why the separate OwningTenantId
     * column that used to duplicate it was dropped.
     */

    public class TenantRoleConfiguration : IEntityTypeConfiguration<TenantRole>
    {
        public void Configure(EntityTypeBuilder<TenantRole> builder)
        {
            builder.ToTable("TenantRole", "Core");
            builder.HasKey(r => r.Id);
            // SRMS declares this alternate key so composite foreign keys can target it.
            builder.HasAlternateKey(r => new { r.Id, r.TenantId }).HasName("AK_TenantRole_Id_TenantId");

            builder.Property(r => r.Code).IsRequired().HasMaxLength(80);
            builder.Property(r => r.Name).IsRequired().HasMaxLength(100);
            builder.Property(r => r.Description);   // nvarchar(max), as in SRMS

            // The template is a soft link: deleting a global Role must not delete tenants' instances,
            // which may since have been customised.
            builder.HasOne<Role>().WithMany()
                .HasForeignKey(r => r.SourceTemplateId).OnDelete(DeleteBehavior.SetNull);

            // SRMS names this column RoleId; the property stays SourceTemplateId because "RoleId" on
            // a table of roles reads like a primary key. Mapped, not renamed (2026-08-15).
            builder.Property(r => r.SourceTemplateId).HasColumnName("RoleId");

            builder.HasIndex(r => new { r.TenantId, r.Code }).IsUnique();
        }
    }

    /// <summary>
    /// Core.TenantModule — the tenant's copy of a menu group (2026-08-15, SRMS parity). Column
    /// lengths are SRMS's: Name nvarchar(200), Filter nvarchar(500), Icon nvarchar(100).
    /// </summary>
    public class TenantModuleConfiguration : IEntityTypeConfiguration<TenantModule>
    {
        public void Configure(EntityTypeBuilder<TenantModule> builder)
        {
            builder.ToTable("TenantModule", "Core");
            builder.HasKey(m => m.Id).HasName("PK_TenantNavigationModule");   // SRMS's name

            builder.Property(m => m.Name).IsRequired().HasMaxLength(200);
            builder.Property(m => m.Icon).IsRequired().HasMaxLength(100);
            builder.Property(m => m.Filter).IsRequired().HasMaxLength(500);
            builder.Property(m => m.UpdatedAt).HasColumnType("datetime2(3)");

            builder.HasOne<Subsystem>().WithMany()
                .HasForeignKey(m => m.SubSystemId).OnDelete(DeleteBehavior.Restrict);

            // No template link — SRMS keeps none. A copy is keyed to its template by
            // (SubSystemId, Name), which is unique in both tables.
            builder.HasIndex(m => new { m.TenantId, m.SubSystemId, m.Name }).IsUnique();
        }
    }

    public class TenantOperationConfiguration : IEntityTypeConfiguration<TenantOperation>
    {
        public void Configure(EntityTypeBuilder<TenantOperation> builder)
        {
            builder.ToTable("TenantOperation", "Core");
            builder.HasKey(o => o.Id);

            builder.Property(o => o.Name).IsRequired().HasMaxLength(200);
            builder.Property(o => o.Link).IsRequired().HasMaxLength(500);
            builder.Property(o => o.Icon).IsRequired().HasMaxLength(100);
            builder.Property(o => o.Filter).IsRequired().HasMaxLength(500).HasDefaultValue(string.Empty);
            builder.Property(o => o.UpdatedAt).HasColumnType("datetime2(3)");

            // ⚠️ TenantId is GONE (2026-08-15), matching SRMS: the tenant lives on the GROUP, and a
            // screen's tenant is its module's. Nothing here can be filtered by tenant directly —
            // see the warning in Repository.IsGlobalEntity.
            builder.Ignore(o => o.TenantId);

            // ModuleId now points at the TENANT's group, not the global module, and is NOT NULL:
            // every row here is a screen since groups moved to TenantModule (2026-08-15).
            builder.HasOne<TenantModule>().WithMany()
                .HasForeignKey(o => o.ModuleId).OnDelete(DeleteBehavior.NoAction);

            // One copy per (module, link) — the natural key now that OperationId is gone.
            builder.HasIndex(o => new { o.ModuleId, o.Link }).IsUnique();
            // The permission check resolves by LINK, so that lookup gets its own index.
            builder.HasIndex(o => o.Link);
        }
    }

    public class TenantRolePermissionConfiguration : IEntityTypeConfiguration<TenantRolePermission>
    {
        public void Configure(EntityTypeBuilder<TenantRolePermission> builder)
        {
            builder.ToTable("TenantRolePermission", "Core");
            builder.HasKey(p => p.Id);

            builder.HasOne<TenantRole>().WithMany()
                .HasForeignKey(p => p.TenantRoleId).OnDelete(DeleteBehavior.Cascade);
            // NoAction on the second leg: two cascade paths into the same table is a multiple-cascade
            // -path error in SQL Server, and the role side is the one that should cascade.
            builder.HasOne<TenantOperation>().WithMany()
                .HasForeignKey(p => p.TenantOperationId).OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(p => new { p.TenantRoleId, p.TenantOperationId }).IsUnique();
        }
    }

    public class TenantUserConfiguration : IEntityTypeConfiguration<TenantUser>
    {
        public void Configure(EntityTypeBuilder<TenantUser> builder)
        {
            builder.ToTable("TenantUser", "Core");
            builder.HasKey(u => u.Id);
            // SRMS declares this alternate key so composite foreign keys can target it.
            builder.HasAlternateKey(u => new { u.Id, u.TenantId }).HasName("AK_TenantUser_Id_TenantId");

            builder.Property(u => u.Status).IsRequired().HasMaxLength(30);

            builder.HasOne<User>().WithMany()
                .HasForeignKey(u => u.UserId).OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(u => new { u.TenantId, u.UserId }).IsUnique();
            builder.HasIndex(u => u.UserId);
        }
    }

    public class TenantUserRoleConfiguration : IEntityTypeConfiguration<TenantUserRole>
    {
        public void Configure(EntityTypeBuilder<TenantUserRole> builder)
        {
            builder.ToTable("TenantUserRole", "Core");
            builder.HasKey(r => r.Id);

            builder.Property(r => r.AssignedBy);   // uniqueidentifier, as in SRMS

            builder.HasOne<TenantUser>().WithMany()
                .HasForeignKey(r => r.TenantUserId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<TenantRole>().WithMany()
                .HasForeignKey(r => r.TenantRoleId).OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(r => new { r.TenantUserId, r.TenantRoleId }).IsUnique();
        }
    }

    public class TenantSubSystemConfiguration : IEntityTypeConfiguration<TenantSubSystem>
    {
        public void Configure(EntityTypeBuilder<TenantSubSystem> builder)
        {
            builder.ToTable("TenantSubSystem", "Core");
            builder.HasKey(s => s.Id).HasName("PK_TenantModuleEntitlement");   // SRMS's name

            builder.Property(s => s.SourceType).IsRequired().HasMaxLength(30);
            builder.Property(s => s.Status).IsRequired().HasMaxLength(30);

            builder.HasOne<Subsystem>().WithMany()
                .HasForeignKey(s => s.SubSystemId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(s => new { s.TenantId, s.SubSystemId }).IsUnique();
        }
    }
}
