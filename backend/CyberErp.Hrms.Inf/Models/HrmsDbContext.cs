using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using CyberErp.Hrms.Dom.Entities.Core;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using CyberErp.Hrms.Inf.Models.EntityConfiguration;
using NodaTime;

namespace CyberErp.Hrms.Inf.Models;

public class HrmsDbContext : MultiTenantDbContext
{
    public HrmsDbContext(DbContextOptions<HrmsDbContext> options, IMultiTenantContextAccessor<AppTenantInfo>? tenantContextAccessor = null)
        : base(tenantContextAccessor, options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var basePath = AppContext.BaseDirectory;
            var appsettingsPath = Path.Combine(basePath, "CyberErp.Hrms.Api", "appsettings.json");
            if (!File.Exists(appsettingsPath))
                appsettingsPath = Path.Combine(basePath, "appsettings.json");

            if (File.Exists(appsettingsPath))
            {
                var json = File.ReadAllText(appsettingsPath);
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.TryGetProperty("ConnectionStrings", out var connStrings) &&
                    connStrings.TryGetProperty("DefaultConnection", out var defaultConn))
                {
                    var conn = defaultConn.GetString();
                    if (!string.IsNullOrEmpty(conn))
                    {
                        optionsBuilder.UseSqlServer(conn, b => b.MigrationsAssembly("CyberErp.Hrms.Inf"));
                    }
                }
            }
        }
    }

    public DbSet<User> User { get; set; }
    public DbSet<Tenant> Tenant { get; set; }
    public DbSet<SubscriptionPlan> SubscriptionPlan { get; set; }
    public DbSet<TenantSubscription> TenantSubscription { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("Core");

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.ClrType.GetProperties())
            {
                if (property.PropertyType == typeof(Guid))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property(property.Name)
                        .HasColumnType("uniqueidentifier");
                }
                else if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(Instant))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property(property.Name)
                        .HasColumnType("datetime2(3)");
                }
                else if (property.PropertyType == typeof(DateTime?) || property.PropertyType == typeof(Instant?))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property(property.Name)
                        .HasColumnType("datetime2(7)");
                }
                else if (property.PropertyType == typeof(byte[]) && property.Name == "RowVersion")
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property(property.Name)
                        .HasColumnType("varbinary(8)")
                        .IsConcurrencyToken()
                        .IsRequired();
                }
            }
        }

        var instantConverter = new ValueConverter<Instant, DateTime>(
            v => v.ToDateTimeUtc(),
            v => Instant.FromDateTimeUtc(DateTime.SpecifyKind(v, DateTimeKind.Utc)));

        var nullableInstantConverter = new ValueConverter<Instant?, DateTime?>(
            v => v.HasValue ? v.Value.ToDateTimeUtc() : null,
            v => v.HasValue ? Instant.FromDateTimeUtc(DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)) : null);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.ClrType.GetProperties().Where(p => p.PropertyType == typeof(Instant)))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(property.Name)
                    .HasConversion(instantConverter);
            }

            foreach (var property in entityType.ClrType.GetProperties().Where(p => p.PropertyType == typeof(Instant?)))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(property.Name)
                    .HasConversion(nullableInstantConverter);
            }
        }

        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new TenantConfiguration());
        modelBuilder.ApplyConfiguration(new SubscriptionPlanConfiguration());
        modelBuilder.ApplyConfiguration(new TenantSubscriptionConfiguration());
    }
}
