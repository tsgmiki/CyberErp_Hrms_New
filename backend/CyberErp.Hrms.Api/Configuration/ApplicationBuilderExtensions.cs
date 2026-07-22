using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using CyberErp.Hrms.Api.Middleware;
using CyberErp.Hrms.Api.Endpoints;

namespace CyberErp.Hrms.Api.Configuration
{

    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseHrmsSwagger(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            return app;
        }

        public static WebApplication UseHrmsMiddlewarePipeline(this WebApplication app)
        {
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseResponseCompression();
            app.UseForwardedHeaders();
            app.UseRouting();
            app.UseCors("AllowFrontend");
            app.UseMultiTenant();
            app.UseSession();
            app.UseHrmsAuthentication();
            app.UseAuthorization();

            // After authentication: the dashboard's filter needs the resolved user.
            app.UseHrmsBackgroundJobsDashboard();

            app.MapControllers();
            app.MapAccountEndpoints();

            app.MapGet("/test-cors", () => new { Message = "CORS test successful!", Timestamp = DateTime.UtcNow })
               .RequireCors("AllowFrontend");

            return app;
        }
    }
}