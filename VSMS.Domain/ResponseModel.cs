namespace VSMS.Domain;

public class ResponseModel<T>
{
    public bool Succeeded { get; set; }
    public T? Value { get; set; }
    public Error? Error { get; set; }
}

public abstract class Error
{
    public string Property { get; set; }
    public List<string> Description { get; set; } 
}