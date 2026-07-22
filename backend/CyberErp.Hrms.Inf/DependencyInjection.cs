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
            // Generic report engine: Dapper SP execution with ambient tenant/branch/user injection.
            services.AddScoped<App.Features.Core.Reports.IReportExecutor, ReportExecutor>();
            services.AddScoped<App.Features.Core.Reports.IReportJobScheduler, ReportJobScheduler>();
            services.AddScoped<App.Features.Core.Reports.IReportScheduleStore, ReportScheduleStore>();
            // Race-safe per-tenant business numbering (logic.md §7.1 adoption #5)
            services.AddScoped<INumberSequenceService, NumberSequenceService>();
            // Outbound e-mail (Email config section): the app enqueues (QueuedEmailService — cheap
            // guards, returns immediately); the Hangfire EmailDispatchJob performs the SMTP send
            // off the request path via SmtpEmailService, with automatic retries on failure.
            services.AddScoped<SmtpEmailService>();
            services.AddScoped<EmailDispatchJob>();
            services.AddScoped<IEmailService, QueuedEmailService>();
            // PDF letter rendering (QuestPDF) — stateless, safe as a singleton
            services.AddSingleton<IPdfService, QuestPdfService>();

            services.AddScoped<IExceptionHandler, ExceptionHandler>();
            services.AddScoped<ExceptionHandler>();

            // Audit trail interceptor (records structural mutations)
            services.AddScoped<AuditSaveChangesInterceptor>();

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork>(sp => new UnitOfWork(sp.GetRequiredService<Models.HrmsDbContext>()));
            services.AddScoped<IAuthentication, Authentication>();
            services.AddScoped<ITokenStore, TokenStore>();
            services.AddScoped<ITokenParser, TokenParser>();

            // Users - Auth
            services.AddScoped<IGetCurrentUserRepository, GetCurrentUserRepository>();
            services.AddScoped<ILoginRepository, LoginRepository>();
            services.AddScoped<IRegisterRepository, RegisterRepository>();

            // Operations - GetAll using repository pattern
            services.AddScoped<IGetAllOperationsRepository, GetAllOperationsRepository>();

            // Dynamic navigation — module list + permission-filtered menu feed
            services.AddScoped<App.Features.Core.Modules.GetAll.IGetAllModuleRepository,
                Repositories.Core.Modules.GetAllModuleRepository>();
            services.AddScoped<App.Features.Core.Modules.GetOperations.IGetModuleWithOperationsRepository,
                Repositories.Core.Modules.GetModuleWithOperationsRepository>();

            return services;
        }
    }
}
