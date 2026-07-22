using CyberErp.Hrms.App;
using CyberErp.Hrms.Inf;
using CyberErp.Hrms.Api.Configuration;
using Serilog;


Log.Logger = new LoggerConfiguration()
    .ConfigureSerilog()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services
    .AddHrmsForwardedHeaders()
    .AddHrmsSession()
    .AddHrmsAuthentication(builder.Configuration)
    .AddHrmsControllers()
    .AddHrmsCors(builder.Configuration)
    .AddHrmsValidators()
    .AddHrmsDbContext(builder.Configuration)
    .AddHrmsMultiTenancy()
    .AddHrmsBackgroundJobs(builder.Configuration)
    .AddHrmsResponseCompression()
    .AddInfrastractureServices()
    .AddApplicationServices()
    .AddHrmsSwagger()
    .AddHrmsApiVersioning();

var app = builder.Build();

app.UseHrmsSwagger(app.Environment);
app.UseHrmsMiddlewarePipeline();

app.Run();
