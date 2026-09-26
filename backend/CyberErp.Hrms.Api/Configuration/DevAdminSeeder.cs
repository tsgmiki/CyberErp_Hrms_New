using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using UserEntity = CyberErp.Hrms.Dom.Entities.Core.User;

namespace CyberErp.Hrms.Api.Configuration
{
    /// <summary>
    /// Creates ONE named development account in an existing tenant, so the API can be exercised
    /// end-to-end without inventing a tenant or knowing anybody's real password.
    /// </summary>
    /// <remarks>
    /// <para>⚠️ OFF UNLESS EXPLICITLY CONFIGURED, and three separate things must all be true: the
    /// host must be in Development, <c>DevAdmin:Enabled</c> must be true, and a non-empty
    /// <c>DevAdmin:Password</c> must be supplied. Missing any one of them and this does nothing at
    /// all — there is no default password and no default account name.</para>
    ///
    /// <para>⚠️ CREATE-ONLY, BY DESIGN. If the account already exists it is left completely alone —
    /// this never rewrites a password, never re-enables a disabled membership and never touches any
    /// other account. That is the difference between a seeder and a back door: a startup path that
    /// could reset an existing user's credentials would be a way into any account on the system,
    /// and no configuration flag makes that acceptable. To rotate the dev password, delete the
    /// account and let it be seeded again.</para>
    ///
    /// <para>It grants the role named by <c>DevAdmin:RoleName</c>. The role must already exist in
    /// the tenant and carry the privileges — this seeds an account, not a permission set.</para>
    /// </remarks>
    public static class DevAdminSeeder
    {
        public static async Task SeedDevAdminAsync(this IServiceProvider services, IConfiguration configuration,
            IHostEnvironment environment, ILogger logger)
        {
            if (!environment.IsDevelopment()) return;

            var section = configuration.GetSection("DevAdmin");
            if (!section.GetValue("Enabled", false)) return;

            var tenantIdentifier = section["TenantIdentifier"];
            var userName = section["UserName"];
            var password = section["Password"];
            var roleName = section["RoleName"];
            var email = section["Email"];
            var phone = section["PhoneNumber"];

            if (string.IsNullOrWhiteSpace(tenantIdentifier) || string.IsNullOrWhiteSpace(userName)
                || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(roleName))
            {
                logger.LogWarning("DevAdmin seeding is enabled but incomplete — "
                    + "TenantIdentifier, UserName, Password and RoleName are all required. Skipping.");
                return;
            }

            using var scope = services.CreateScope();
            var sp = scope.ServiceProvider;
            var users = sp.GetRequiredService<IRepository<UserEntity>>();
            var tenants = sp.GetRequiredService<IRepository<Tenant>>();
            var tenantUsers = sp.GetRequiredService<IRepository<TenantUser>>();
            var tenantRoles = sp.GetRequiredService<IRepository<TenantRole>>();
            var tenantUserRoles = sp.GetRequiredService<IRepository<TenantUserRole>>();
            var authentication = sp.GetRequiredService<IAuthentication>();

            var tenant = await tenants.GetAllWithoutTenantFilter().AsNoTracking()
                .FirstOrDefaultAsync(t => t.Identifier == tenantIdentifier);
            if (tenant is null)
            {
                logger.LogWarning("DevAdmin seeding: no tenant with identifier '{Identifier}'.", tenantIdentifier);
                return;
            }

            // Create-only. An existing account is evidence the seed already ran (or that the name
            // collides with a real user); either way it is not this code's business to change it.
            var existing = await users.GetAllWithoutTenantFilter().AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserName == userName);
            if (existing is not null)
            {
                logger.LogInformation("DevAdmin seeding: '{UserName}' already exists — left untouched.", userName);
                return;
            }

            var role = await tenantRoles.GetAllWithoutTenantFilter().AsNoTracking()
                .FirstOrDefaultAsync(r => r.Name == roleName && r.TenantId == tenant.Id.ToString());
            if (role is null)
            {
                logger.LogWarning("DevAdmin seeding: tenant '{Identifier}' has no role named '{RoleName}'.",
                    tenantIdentifier, roleName);
                return;
            }

            var user = UserEntity.Create(
                fullName: "Development Administrator",
                email: string.IsNullOrWhiteSpace(email) ? $"{userName}@localhost.dev" : email,
                phoneNumber: string.IsNullOrWhiteSpace(phone) ? "0000000000" : phone,
                userName: userName,
                password: authentication.EncryptPassword(password));
            user.TenantId = tenant.Id.ToString();
            await users.AddAsync(user);
            await users.SaveChangesAsync();

            // Login resolves the session tenant from this row, not from User.TenantId.
            var membership = TenantUser.Create(tenant.Id, user.Id, status: true, isDefaultTenant: true);
            await tenantUsers.AddAsync(membership);
            await tenantUsers.SaveChangesAsync();

            await tenantUserRoles.AddAsync(TenantUserRole.Create(membership.Id, role.Id));
            await tenantUserRoles.SaveChangesAsync();

            logger.LogWarning("DevAdmin seeded: '{UserName}' in tenant '{Identifier}' with role '{RoleName}'. "
                + "This account exists only because DevAdmin:Enabled is true in a Development host.",
                userName, tenantIdentifier, roleName);
        }
    }
}
