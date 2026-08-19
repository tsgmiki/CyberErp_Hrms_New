using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    /*
     * Platform layer ported from the SRMS schema (Organization / subscription / operations).
     *
     * These tables live in the Core schema because they are shared-platform concerns, not HRMS ones.
     * None of them is marked [MultiTenant]: an Organization spans tenants, plan-to-module mapping and
     * add-ons are billing records ABOUT tenants that platform staff must read across all of them, and
     * Setting is a deployment singleton. UserPreference and LoginTrail DO carry the BaseEntity tenant
     * discriminator and are written per tenant.
     */

    public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
    {
        public void Configure(EntityTypeBuilder<Organization> builder)
        {
            builder.ToTable("Organization", "Core");
            builder.HasKey(o => o.Id);

            // TenantId is GONE (2026-08-15) — SRMS has none. An organization is the legal entity a
            // tenant belongs to, so a tenant discriminator on it was backwards. Already in
            // Repository.IsGlobalEntity, so nothing ever filtered on it.
            builder.Ignore(o => o.TenantId);

            builder.Property(o => o.Code).IsRequired().HasMaxLength(80);
            builder.Property(o => o.LegalName).IsRequired().HasMaxLength(200);
            builder.Property(o => o.DisplayName).IsRequired().HasMaxLength(200);

            builder.Property(o => o.Address).HasMaxLength(500);
            builder.Property(o => o.PostalAddress).HasMaxLength(500);
            builder.Property(o => o.PostalCode).HasMaxLength(30);
            builder.Property(o => o.PhoneNumber).HasMaxLength(50);
            builder.Property(o => o.Email).HasMaxLength(200);
            builder.Property(o => o.Website).HasMaxLength(300);
            builder.Property(o => o.City).HasMaxLength(100);
            builder.Property(o => o.Region).HasMaxLength(100);
            builder.Property(o => o.Country).HasMaxLength(100);

            builder.Property(o => o.PrimaryContactName).HasMaxLength(150);
            builder.Property(o => o.PrimaryContactTitle).HasMaxLength(100);
            builder.Property(o => o.PrimaryContactEmail).HasMaxLength(200);
            builder.Property(o => o.PrimaryContactPhone).HasMaxLength(50);

            builder.Property(o => o.RegistrationNumber).HasMaxLength(100);
            builder.Property(o => o.TaxNumber).HasMaxLength(100);
            builder.Property(o => o.TINNumber).HasMaxLength(50);
            builder.Property(o => o.RegulatoryIdentifiers).HasMaxLength(1000);
            builder.Property(o => o.Industry).HasMaxLength(150);
            builder.Property(o => o.OrganizationType).HasMaxLength(100);

            // Fixed width in the source schema; ISO 4217 is always three characters.
            builder.Property(o => o.Currency).IsRequired().HasColumnType("nchar(3)");
            builder.Property(o => o.Timezone).IsRequired().HasMaxLength(100);
            builder.Property(o => o.Locale).IsRequired().HasMaxLength(20);
            builder.Property(o => o.DefaultLanguage).IsRequired().HasMaxLength(20).HasDefaultValue("en");
            builder.Property(o => o.DateFormat).IsRequired().HasMaxLength(30);
            builder.Property(o => o.FiscalYearStartMonth).HasDefaultValue(1);

            builder.Property(o => o.LogoContentType).HasMaxLength(100);
            builder.Property(o => o.DataRetentionPolicy).HasMaxLength(1000);

            // One organization per code — the code is how a deployment is identified.
            builder.HasIndex(o => o.Code).IsUnique();
        }
    }

    public class OrganizationSubscriptionConfiguration : IEntityTypeConfiguration<OrganizationSubscription>
    {
        public void Configure(EntityTypeBuilder<OrganizationSubscription> builder)
        {
            builder.ToTable("OrganizationSubscription", "Core");
            builder.HasKey(s => s.Id);
            // SRMS declares this alternate key so composite foreign keys can target it.
            builder.HasAlternateKey(s => new { s.Id, s.OrganizationId }).HasName("AK_OrganizationSubscription_Id_OrganizationId");

            // ⚠️ TenantId is GONE (2026-08-15) — SRMS has none, and this is PLATFORM data, not
            // tenant data: a plan and its modules belong to the product, not to one customer. The
            // table is empty, so nothing was lost. Added to Repository.IsGlobalEntity in the same
            // change, WITHOUT which every read fails on the now-unmapped filter member.
            builder.Ignore(s => s.TenantId);


            builder.Property(s => s.Status).IsRequired().HasMaxLength(30);
            builder.Property(s => s.Currency).IsRequired().HasColumnType("nchar(3)");

            builder.HasOne<Organization>()
                .WithMany()
                .HasForeignKey(s => s.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<SubscriptionPlan>()
                .WithMany()
                .HasForeignKey(s => s.SubscriptionPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(s => s.OrganizationId);
            builder.HasIndex(s => s.Status);
        }
    }

    public class SubscriptionPlanModuleConfiguration : IEntityTypeConfiguration<SubscriptionPlanModule>
    {
        public void Configure(EntityTypeBuilder<SubscriptionPlanModule> builder)
        {
            builder.ToTable("SubscriptionPlanModule", "Core");
            builder.HasKey(m => m.Id);

            // ⚠️ TenantId is GONE (2026-08-15) — SRMS has none, and this is PLATFORM data, not
            // tenant data: a plan and its modules belong to the product, not to one customer. The
            // table is empty, so nothing was lost. Added to Repository.IsGlobalEntity in the same
            // change, WITHOUT which every read fails on the now-unmapped filter member.
            builder.Ignore(m => m.TenantId);


            builder.HasOne<SubscriptionPlan>()
                .WithMany()
                .HasForeignKey(m => m.SubscriptionPlanId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<Module>()
                .WithMany()
                .HasForeignKey(m => m.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);

            // A module appears at most once in a plan.
            builder.HasIndex(m => new { m.SubscriptionPlanId, m.ModuleId }).IsUnique();
        }
    }

    public class TenantSubscriptionAddOnConfiguration : IEntityTypeConfiguration<TenantSubscriptionAddOn>
    {
        public void Configure(EntityTypeBuilder<TenantSubscriptionAddOn> builder)
        {
            builder.ToTable("TenantSubscriptionAddOn", "Core");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Status).IsRequired().HasMaxLength(30);
            builder.Property(a => a.Currency).IsRequired().HasMaxLength(3);
            builder.Property(a => a.Amount).HasPrecision(18, 2);

            // SubscribedTenantId is a real FK to Core.Tenant, distinct from BaseEntity.TenantId.
            builder.HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(a => a.SubscribedTenantId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<Module>()
                .WithMany()
                .HasForeignKey(a => a.ModuleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(a => new { a.SubscribedTenantId, a.Status });
        }
    }

    public class LoginTrailConfiguration : IEntityTypeConfiguration<LoginTrail>
    {
        public void Configure(EntityTypeBuilder<LoginTrail> builder)
        {
            builder.ToTable("LoginTrail", "Core");
            builder.HasKey(l => l.Id);

            builder.Property(l => l.UserNameAttempted).IsRequired().HasMaxLength(200).HasDefaultValue(string.Empty);
            builder.Property(l => l.EventType).IsRequired().HasMaxLength(30).HasDefaultValue("Login");

            // SRMS constrains UserId to Core.User with SET NULL, so a deleted account leaves its
            // audit trail behind with the link cleared rather than taking the rows with it. CERP had
            // no constraint at all. Verified before adding: 84 rows, 0 orphans, column nullable.
            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .HasConstraintName("FK_LoginTrail_User_UserId")
                .OnDelete(DeleteBehavior.SetNull);
            builder.Property(l => l.IpAddress).IsRequired().HasMaxLength(45);
            builder.Property(l => l.Status).HasMaxLength(50);
            builder.Property(l => l.FailureReason).HasMaxLength(500);
            builder.Property(l => l.UserAgent).HasMaxLength(1000);

            // Deliberately NO foreign key to Core.User: a failed attempt against an unknown name has
            // no user to point at, and the trail must outlive a deleted account — an audit row that
            // disappears with its subject is not an audit row.
            builder.HasIndex(l => l.UserId);
            builder.HasIndex(l => l.Date);
            builder.HasIndex(l => new { l.UserNameAttempted, l.EventType });

            // ⚠️ THE INDEX THAT MATTERS AT SCALE. Every "recent security activity" read is
            // `WHERE UserId = @x ORDER BY Date DESC` + TOP(n) — the Edit Profile dialog does it on
            // open, for every user. With only IX_LoginTrail_UserId, SQL Server seeks the user then
            // SORTS every row they have ever accumulated just to take the newest few. This table
            // grows one row per sign-in ATTEMPT and is never trimmed, so that sort grows without
            // bound per user. Leading with UserId and descending on Date makes it a seek + top,
            // with no sort at all.
            builder.HasIndex(l => new { l.UserId, l.Date })
                .IsDescending(false, true)
                .HasDatabaseName("IX_LoginTrail_UserId_Date");
        }
    }

    public class SettingConfiguration : IEntityTypeConfiguration<Setting>
    {
        public void Configure(EntityTypeBuilder<Setting> builder)
        {
            // SRMS keeps this at datetime2(3); the convention gives non-nullable stamps (3) but
            // Setting.UpdatedAt is non-nullable AND was (7) from an older explicit mapping.
            builder.HasKey(x => x.Id).HasName("PK_SystemSetting");   // SRMS's constraint name

            // TenantId is GONE (2026-08-15) — SRMS has none, the single row held the empty Guid, and
            // Setting has always been in Repository.IsGlobalEntity so nothing filtered on it.
            builder.Ignore(x => x.TenantId);

            builder.Property(x => x.UpdatedAt).HasColumnType("datetime2(3)");

            builder.ToTable("Setting", "Core");
            builder.HasKey(s => s.Id);

            builder.Property(s => s.SmtpHost).IsRequired().HasMaxLength(255);
            builder.Property(s => s.SmtpUser).IsRequired().HasMaxLength(255);
            builder.Property(s => s.BackupFrequency).IsRequired().HasMaxLength(20);

            // SRMS alignment (2026-08-14): UpdatedAt is NOT NULL here, unlike everywhere else.
            // BaseEntity leaves it nullable in the CLR, which is right — a row that has never been
            // updated has no update time — so the column is required at the database level only,
            // and the migration seeds existing rows from CreatedAt.
            builder.Property(s => s.UpdatedAt).IsRequired();
        }
    }

    public class UserPreferenceConfiguration : IEntityTypeConfiguration<UserPreference>
    {
        public void Configure(EntityTypeBuilder<UserPreference> builder)
        {
            builder.ToTable("UserPreference", "Core");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Language).IsRequired().HasMaxLength(10).HasDefaultValue("en");
            // SRMS alignment (2026-08-14): required and narrower. The table is empty, so the
            // conversion costs nothing; defaults keep inserts working without a value.
            builder.Property(p => p.TimeZone).IsRequired().HasMaxLength(100).HasDefaultValue("Africa/Nairobi");
            builder.Property(p => p.DateFormat).IsRequired().HasMaxLength(30).HasDefaultValue("dd/MM/yyyy");
            builder.Property(p => p.NumberFormat).IsRequired().HasMaxLength(30).HasDefaultValue("1,234.56");
            builder.Property(p => p.LandingPage).IsRequired().HasMaxLength(200).HasDefaultValue("/");
            builder.Property(p => p.Theme).IsRequired().HasMaxLength(20).HasDefaultValue("system");
            builder.Property(p => p.EmailNotifications).HasDefaultValue(true);
            builder.Property(p => p.InAppNotifications).HasDefaultValue(true);
            builder.Property(p => p.ApprovalNotifications).HasDefaultValue(true);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // One preference row per user per tenant.
            builder.HasIndex(p => new { p.UserId, p.TenantId }).IsUnique();
        }
    }
}
