using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.Extensions.DependencyInjection;
using VSMS.Infrastructure.Helpers;
using VSMS.Infrastructure.Hubs;
using VSMS.Infrastructure.Services.HttpServices;

namespace VSMS.Infrastructure.Extensions;

public static class InfrastructureCollectionExtensions
{
    public static IServiceCollection AddBlazoredConfiguration(this IServiceCollection services)
    {
        services.AddBlazoredLocalStorage();
        services.AddBlazoredSessionStorage();
        return services;
    }
    
    public static IServiceCollection AddHelpersConfiguration(this IServiceCollection services)
    {
        services.AddTransient<WebPageHelper>();
        services.AddTransient<CookieHelper>();
        services.AddScoped<TimeZoneHelper>();
        
        return services;
    }

    public static IServiceCollection AddHttpServicesConfiguration(this IServiceCollection services)
    {
        services.AddTransient<AuthHttpService>();
        services.AddTransient<CompaniesHttpService>();
        services.AddTransient<StocksHttpService>();
        services.AddTransient<UsersHttpService>();
        
        return services;
    }
    
    public static IServiceCollection AddHubsConfiguration(this IServiceCollection services)
    {
        services.AddTransient<ApplicationHub>();
        services.AddTransient<StocksHub>();
        
        return services;
    }
}