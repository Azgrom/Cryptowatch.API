using System;

namespace CoreAbstractions;

public class Result
{
    private protected Result(bool isSuccess, Error err)
    {
        switch (isSuccess)
        {
            case true when err  != Error.None: throw new InvalidOperationException();
            case false when err == Error.None: throw new InvalidOperationException();
            default:
                IsSuccess = isSuccess;
                Error     = err;
                break;
        }
    }

    public        bool           IsSuccess                     { get; }
    public        bool           IsFailure                     => !IsSuccess;
    public        Error          Error                         { get; }
    public static Result         Success()                     => new(true, Error.None);
    public static Result         Failure()                     => new(false, Error.NullValue);
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);
    public static Result<TValue> Failure<TValue>(Error  err)   => new((TValue?)(object)null!, false, err);
}

public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    public Result(TValue? value, bool isSuccess, Error err)
        : base(isSuccess, err) =>
        _value = value;


    public TValue Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException("The value of a failure result cannot be accessed");

    public static implicit operator Result<TValue>(TValue value) => Success(value);
    public static implicit operator Result<TValue>(Error  err)   => Failure<TValue>(err);
}
