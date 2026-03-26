using Anir.Shared.Enums;

public class ProcessResponse<T>
{
    public T? Value { get; set; }
    public ResponseStatus Result { get; set; }
    public string? PrettyMessage { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, string[]>? ValidationErrors { get; set; }

    public static ProcessResponse<T> Success(T? value = default, string? message = null)
        => new()
        {
            Result = ResponseStatus.Success,
            Value = value,
            PrettyMessage = message
        };

    public static ProcessResponse<T> Fail(string error, Dictionary<string, string[]>? validation = null)
        => new()
        {
            Result = ResponseStatus.Failed,
            ErrorMessage = error,
            ValidationErrors = validation
        };
}
