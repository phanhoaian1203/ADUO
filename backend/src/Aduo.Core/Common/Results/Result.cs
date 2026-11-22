using System.Diagnostics.CodeAnalysis;

namespace Aduo.Core.Common.Results;

/// <summary>
/// Kết quả của một thao tác nghiệp vụ không trả về giá trị: thành công, hoặc thất bại kèm <see cref="Error"/>.
/// Dùng thay cho exception với các lỗi đã lường trước (ví dụ "phần thi đã hết giờ").
/// </summary>
public readonly record struct Result
{
    private Result(Error? error) => Error = error;

    public Error? Error { get; }

    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess => Error is null;

    public static Result Ok() => new(null);

    public static Result Fail(Error error) => new(error ?? throw new ArgumentNullException(nameof(error)));

    public static implicit operator Result(Error error) => Fail(error);
}

/// <summary>
/// Kết quả có giá trị: <see cref="Value"/> khi thành công, <see cref="Error"/> khi thất bại.
/// </summary>
public readonly record struct Result<T>
{
    private Result(T? value, Error? error)
    {
        Value = value;
        Error = error;
    }

    public T? Value { get; }

    public Error? Error { get; }

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess => Error is null;

    public static Result<T> Ok(T value) => new(value, null);

    public static Result<T> Fail(Error error) => new(default, error ?? throw new ArgumentNullException(nameof(error)));

    public static implicit operator Result<T>(T value) => Ok(value);

    public static implicit operator Result<T>(Error error) => Fail(error);

    /// <summary>Biến đổi giá trị khi thành công, giữ nguyên lỗi khi thất bại.</summary>
    public Result<TOut> Map<TOut>(Func<T, TOut> map) => IsSuccess ? Result<TOut>.Ok(map(Value)) : Result<TOut>.Fail(Error);
}
