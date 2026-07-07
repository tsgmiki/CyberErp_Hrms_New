using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Features.Core.Operations.GetAll;
using CyberErp.Hrms.App.Features.Core.Users.GetCurrentUser;
using CyberErp.Hrms.App.Features.Core.Users.Login;
using CyberErp.Hrms.App.Features.Core.Users.Register;
using CyberErp.Hrms.Inf.Common;
using CyberErp.Hrms.Inf.Repositories;
using CyberErp.Hrms.Inf.Repositories.Core;
using CyberErp.Hrms.Inf.Repositories.Core.Operations;
using CyberErp.Hrms.Inf.Repositories.Core.Users;
using Microsoft.Extensions.DependencyInjection;

namespace CyberErp.Hrms.Inf
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastractureServices(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<ITenantService, TenantService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            services.AddScoped<IExceptionHandler, ExceptionHandler>();
            services.AddScoped<ExceptionHandler>();

            // Audit trail interceptor (records structural mutations)
            services.AddScoped<AuditSaveChangesInterceptor>();

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IAuthentication, Authentication>();
            services.AddScoped<ITokenStore, TokenStore>();
            services.AddScoped<ITokenParser, TokenParser>();

            // Users - Auth
            services.AddScoped<IGetCurrentUserRepository, GetCurrentUserRepository>();
            services.AddScoped<ILoginRepository, LoginRepository>();
            services.AddScoped<IRegisterRepository, RegisterRepository>();

            // Operations - GetAll using repository pattern
            services.AddScoped<IGetAllOperationsRepository, GetAllOperationsRepository>();

            return services;
        }
    }
}
