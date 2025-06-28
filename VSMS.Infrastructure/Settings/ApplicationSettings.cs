namespace VSMS.Infrastructure.Settings;

public class ApplicationSettings : IApplicationSettings
{
    public required string ApiUrl { get; set; }
    
    public override string ToString()
    {
        return $"Application Settings:\n\n" +
               $"1. ApiUrl\t-\t{ApiUrl}\n";
    }
}