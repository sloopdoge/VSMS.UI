﻿namespace VSMS.Infrastructure.Settings;

public class ApplicationSettings : IApplicationSettings
{
    public required string ApiUrl { get; set; }
    public required string ApplicationTitle { get; set; }

    public override string ToString()
    {
        return $"Application Settings:\n" +
               $"1. ApiUrl\t-\t{ApiUrl}\n" +
               $"2. ApplicationTitle\t-\t{ApplicationTitle}";
    }
}