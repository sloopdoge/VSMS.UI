using Blazored.LocalStorage;
using Microsoft.Extensions.Logging;
using VSMS.Infrastructure.Settings;

namespace VSMS.Infrastructure.Hubs;

public class ApplicationHub(
    IApplicationSettings settings, 
    ILogger<ApplicationHub> logger,
    ILocalStorageService localStorage) 
    : BaseHub(settings, logger, localStorage, "ApplicationHub")
{
    
}