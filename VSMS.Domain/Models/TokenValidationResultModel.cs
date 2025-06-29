namespace VSMS.Domain.Models;

public class TokenValidationResultModel
{
    public bool IsValid { get; set; }
    public string? Error { get; set; }
}