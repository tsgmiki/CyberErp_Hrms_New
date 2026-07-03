using Microsoft.Extensions.DependencyInjection;
using CyberErp.Hrms.App.Features.Core.Users.Login;
using CyberErp.Hrms.App.Features.Core.Users.Logout;
using CyberErp.Hrms.App.Features.Core.Users.GetCurrentUser;
using CyberErp.Hrms.App.Features.Core.Users.Register;

namespace CyberErp.Hrms.App
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Users - Auth
            services.AddScoped<ILoginUser, LoginUser>();
            services.AddScoped<ILogoutUser, LogoutUser>();
            services.AddScoped<ILogoutCookieUser, LogoutCookieHandler>();
            services.AddScoped<IRegisterUser, RegisterUser>();
            services.AddScoped<IRegisterWithGoogle, RegisterWithGoogle>();
            services.AddScoped<IGetCurrentUser, GetCurrentUser>();

            return services;
        }
    }
}
