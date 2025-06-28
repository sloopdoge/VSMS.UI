namespace VSMS.Domain.Models;

public class TokenModel
{
    public required string Value { get; set; }
    public DateTime Expires { get; set; }
}