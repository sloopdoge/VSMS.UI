using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Grafana.Loki;
using VSMS.Application.Components;
using VSMS.Application.Identity;
using VSMS.Infrastructure.Extensions;
using VSMS.Infrastructure.Settings;

namespace VSMS.Application;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        #region Serilog Logger

        if (builder.Environment.IsDevelopment())
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();
        }

        if (builder.Environment.IsProduction())
        {
            var lokiUri = builder.Configuration.GetValue<string>("LokiSettings:Url");
            var appName = builder.Configuration.GetValue<string>("LokiSettings:AppName");
            var serviceName = builder.Configuration.GetValue<string>("LokiSettings:ServiceName");

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.GrafanaLoki(
                    lokiUri!,
                    labels:
                    [
                        new LokiLabel
                        {
                            Key = "app",
                            Value = appName!
                        },
                        new LokiLabel
                        {
                            Key = "service",
                            Value = serviceName!
                        }
                    ],
                    restrictedToMinimumLevel: LogEventLevel.Information)
                .CreateLogger();
        }

        builder.Logging.ClearProviders();
        builder.Host.UseSerilog();

        #endregion

        Log.Warning("Starting web host");
        try
        {
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
            
            builder.Services.AddAntiforgery(options => 
                options.SuppressXFrameOptionsHeader = true);
            
            builder.Services.AddMudServices();
            
            builder.Services.AddBlazoredConfiguration();
            builder.Services.AddHelpersConfiguration();
            builder.Services.AddHttpServicesConfiguration();
            builder.Services.AddHubsConfiguration();
            
            builder.Services.AddSingleton<IApplicationSettings, ApplicationSettings>( sp => 
                builder.Configuration.GetSection("ApplicationSettings").Get<ApplicationSettings>()!);
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
            
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddAuthorizationCore();
            
            builder.Services.AddLocalization(options =>
                {
                    options.ResourcesPath = "Resources";
                });
            builder.Services.AddMvc()
                .AddViewLocalization()
                .AddDataAnnotationsLocalization();
            
            builder.Services.AddServerSideBlazor()
                .AddCircuitOptions(options => { options.DetailedErrors = true; });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            Log.Information(app.Services.GetService<IApplicationSettings>()!.ToString()!);
            
            var allowedLocalizations = new[]
            {
                "en-us", 
                "uk-ua"
            };
            app.UseRequestLocalization(new RequestLocalizationOptions()
                .SetDefaultCulture("en-us")
                .AddSupportedCultures(allowedLocalizations)
                .AddSupportedUICultures(allowedLocalizations));

            app.Run();
        }
        catch (Exception e)
        {
            Log.Fatal(e, e.Message);
        }
        finally
        {
            Log.Warning("Web host shutdown");
            Log.CloseAndFlush();
        }
    }
}