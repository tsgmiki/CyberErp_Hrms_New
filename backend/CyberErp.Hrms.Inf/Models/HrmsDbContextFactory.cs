using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Text.Json;

namespace CyberErp.Hrms.Inf.Models;

/// <summary>
/// Design-time DbContext factory for migrations
/// </summary>
public class HrmsDbContextFactory : IDesignTimeDbContextFactory<HrmsDbContext>
{
    public HrmsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<HrmsDbContext>();

        var connectionString = GetConnectionString();

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "Database connection string not configured. Set ConnectionStrings:DefaultConnection " +
                "in appsettings.json.");
        }

        optionsBuilder.UseSqlServer(connectionString, b => b.MigrationsAssembly("CyberErp.Hrms.Inf"));

        return new HrmsDbContext(optionsBuilder.Options);
    }

    private static string? GetConnectionString()
    {
        var envConnection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
        if (!string.IsNullOrEmpty(envConnection))
            return envConnection;

        var basePath = Directory.GetCurrentDirectory();
        var appsettingsPath = Path.Combine(basePath, "CyberErp.Hrms.Api", "appsettings.json");

        if (!File.Exists(appsettingsPath))
            appsettingsPath = Path.Combine(basePath, "appsettings.json");

        if (File.Exists(appsettingsPath))
        {
            var json = File.ReadAllText(appsettingsPath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.TryGetProperty("ConnectionStrings", out var connStrings) &&
                connStrings.TryGetProperty("DefaultConnection", out var defaultConn))
            {
                return defaultConn.GetString();
            }
        }

        return null;
    }
}
