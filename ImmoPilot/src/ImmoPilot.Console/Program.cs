using ImmoPilot.Application.Interfaces;
using ImmoPilot.Application.Models;
using ImmoPilot.Application.Options;
using ImmoPilot.Application.Services;
using ImmoPilot.Infrastructure.Http;
using ImmoPilot.Infrastructure.Notifications;
using ImmoPilot.Infrastructure.Options;
using ImmoPilot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Serilog;

var host = Host.CreateDefaultBuilder(args)
    .UseSerilog((ctx, config) => config
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"))
    .ConfigureAppConfiguration((ctx, cfg) =>
    {
        cfg.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
        cfg.AddJsonFile($"appsettings.{ctx.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: false);
        cfg.AddEnvironmentVariables();
    })
    .ConfigureServices((ctx, services) =>
    {
        var cfg = ctx.Configuration;

        // Options
        services.Configure<PipelineOptions>(cfg.GetSection(PipelineOptions.SectionName));
        services.Configure<MarketOptions>(cfg.GetSection(MarketOptions.SectionName));
        services.Configure<DscrThresholdOptions>(cfg.GetSection(DscrThresholdOptions.SectionName));
        services.Configure<NotificationOptions>(cfg.GetSection(NotificationOptions.SectionName));
        services.Configure<ApiOptions>(cfg.GetSection(ApiOptions.SectionName));

        // EF Core
        services.AddDbContext<ImmoPilotDbContext>(options =>
            options.UseSqlServer(cfg.GetConnectionString("ImmoPilot")));

        // Repository
        services.AddScoped<IPropertyRepository, EfPropertyRepository>();

        // HTTP clients with Polly standard resilience
        services.AddHttpClient<IPropertySource, RedfinHttpClient>(client =>
        {
            client.BaseAddress = new Uri(cfg["Api:RedfinBaseUrl"] ?? "https://www.redfin.com");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; ImmoPilot/1.0)");
            client.Timeout = TimeSpan.FromSeconds(30);
        }).AddStandardResilienceHandler();

        services.AddHttpClient<IHudFmrClient, HudFmrHttpClient>(client =>
        {
            client.BaseAddress = new Uri(cfg["Api:HudBaseUrl"] ?? "https://www.huduser.gov");
            client.Timeout = TimeSpan.FromSeconds(15);
        }).AddStandardResilienceHandler();

        // Notifications (DryRun-aware)
        var dryRun = cfg.GetValue<bool>("Pipeline:DryRun");
        if (dryRun)
            services.AddScoped<IWhatsAppNotifier, NullWhatsAppNotifier>();
        else
            services.AddScoped<IWhatsAppNotifier, TwilioWhatsAppNotifier>();

        // Application service
        services.AddScoped<PipelineOrchestrator>();
    })
    .Build();

// Run EF migrations at startup
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ImmoPilotDbContext>();
    await db.Database.MigrateAsync();
}

// Execute pipeline
int exitCode;
using (var scope = host.Services.CreateScope())
{
    var orchestrator = scope.ServiceProvider.GetRequiredService<PipelineOrchestrator>();
    var result = await orchestrator.RunAsync();

    exitCode = (int)result;
    Log.Information("Pipeline finished with result: {Result} (exit code {Code})", result, exitCode);
}

await Log.CloseAndFlushAsync();
Environment.Exit(exitCode);
