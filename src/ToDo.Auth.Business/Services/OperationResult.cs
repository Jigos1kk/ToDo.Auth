namespace ToDo.Auth.Business.Services;

/// <summary>
/// Результат операции: либо значение, либо текст ошибки.
/// </summary>
public record OperationResult<T>
{
    public T? Value { get; private init; }

    public string? Error { get; private init; }

    public bool IsSuccess => Value is not null;

    public static OperationResult<T> Success(T value) => new() { Value = value };

    public static OperationResult<T> Failure(string error) => new() { Error = error };
}
