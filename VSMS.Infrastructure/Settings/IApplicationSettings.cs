namespace VSMS.Infrastructure.Settings;

public interface IApplicationSettings
{
    public string ApiUrl { get; set; }
    public string ApplicationTitle { get; set; }
}