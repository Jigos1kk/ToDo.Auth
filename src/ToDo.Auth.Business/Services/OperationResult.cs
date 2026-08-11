namespace ToDo.Auth.Business.Services;

/// <summary>
/// Результат операции с возвращаемым значением.
/// </summary>
public record OperationResult<T>
{
    public T? Value { get; private init; }

    public string? Error { get; private init; }

    public bool IsSuccess => Value is not null;

    public static OperationResult<T> Success(T value) => new() { Value = value };

    public static OperationResult<T> Failure(string error) => new() { Error = error };
}

/// <summary>
/// Результат операции без возвращаемого значения (только успех или ошибка).
/// </summary>
public record OperationResult
{
    public string? Error { get; private init; }

    public bool IsSuccess => Error is null;

    public static OperationResult Success() => new();

    public static OperationResult Failure(string error) => new() { Error = error };
}