using CyberErp.Hrms.Inf.Models;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CyberErp.Hrms.Api;

public class HrmsDbContextDesignTimeFactory : IDesignTimeDbContextFactory<HrmsDbContext>
{
    public HrmsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<HrmsDbContext>();
        var connectionString =
            "Server=CLOUDX-SICS2\\SQLEXPRESS;Database=Hrms;Trusted_Connection=True;TrustServerCertificate=True;";
        optionsBuilder.UseSqlServer(connectionString, b => b.MigrationsAssembly("CyberErp.Hrms.Inf"));

        var tenantAccessor = new StaticMultiTenantContextAccessor<AppTenantInfo>(new AppTenantInfo
        {
            Id = Guid.Empty.ToString(),
            Identifier = "design",
            Name = "Design Tenant",
            IsActive = true
        });

        return new HrmsDbContext(optionsBuilder.Options, tenantAccessor);
    }
}
