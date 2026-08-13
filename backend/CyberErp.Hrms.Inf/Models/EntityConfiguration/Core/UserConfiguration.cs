using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CyberErp.Hrms.Dom.Entities.Core;

namespace CyberErp.Hrms.Inf.Models.EntityConfiguration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(u => u.PhoneNumber)
                .HasMaxLength(50);

            builder.Property(u => u.UserName)
                .IsRequired()
                .HasMaxLength(100);

            // Renamed from Password in the 2026-08 SRMS alignment.
            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(255);

            // ---- SRMS alignment: normalised lookups + account security --------------------
            builder.Property(u => u.NormalizedUserName)
                .IsRequired().HasMaxLength(100).HasDefaultValue(string.Empty);
            builder.Property(u => u.NormalizedEmail)
                .IsRequired().HasMaxLength(200).HasDefaultValue(string.Empty);

            builder.Property(u => u.AccountStatus)
                .IsRequired().HasMaxLength(20).HasDefaultValue(UserAccountStatuses.Active);
            builder.Property(u => u.FailedLoginAttempts).IsRequired().HasDefaultValue(0);
            builder.Property(u => u.LockoutEndUtc).HasColumnType("datetime2");
            builder.Property(u => u.TwoFactorEnabled).IsRequired().HasDefaultValue(false);
            builder.Property(u => u.IsPlatformAdministrator).IsRequired().HasDefaultValue(false);

            builder.Property(u => u.ProfilePicture).HasColumnType("varbinary(max)");
            builder.Property(u => u.ProfilePictureContentType).HasMaxLength(100);

            // Login resolves users by name BEFORE the tenant is known (LoginRepository) — without
            // this index every sign-in scans the whole User table. SRMS has no such index (it looks
            // up on NormalizedUserName instead); it is kept because dropping it regresses sign-in.
            builder.HasIndex(u => u.UserName);
            builder.HasIndex(u => u.NormalizedUserName).IsUnique();

            // ⚠️ FILTERED, unlike SRMS's plain UNIQUE. 489 of 506 accounts have no e-mail address on
            // file, so a plain unique index cannot be created — they would all collide on ''.
            builder.HasIndex(u => u.NormalizedEmail)
                .IsUnique()
                .HasFilter("[NormalizedEmail] <> ''");

            // NodaTime Instant conversion for CreatedAt
            builder.Property(u => u.CreatedAt)
                .HasConversion(
                    v => v.ToDateTimeUtc(),
                    v => NodaTime.Instant.FromDateTimeUtc(v.ToUniversalTime())
                )
                .HasColumnType("datetime2(3)")
                .IsRequired();

            // NodaTime Instant conversion for UpdatedAt
            builder.Property(u => u.UpdatedAt)
                .HasConversion(
                    v => v.HasValue ? v.Value.ToDateTimeUtc() : (DateTime?)null,
                    v => v.HasValue ? NodaTime.Instant.FromDateTimeUtc(v.Value.ToUniversalTime()) : null
                )
                .HasColumnType("datetime2(3)");

            builder.Property(u => u.RowVersion)
                ;

            // The User owns the relationship to Employee (FK in the User table). SET NULL on
            // employee deletion — the login account survives, just unlinked.
            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(u => u.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}


