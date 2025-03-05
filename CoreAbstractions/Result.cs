using System;

namespace CoreAbstractions;

public readonly record struct Result
{
    private static readonly Result SuccessfulResult = new(true, Error.None);
    private static readonly Result FailureResult    = new(false, Error.NullValue);

    private Result(bool isSuccess, Error err)
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

    public                          bool   IsSuccess     { get; }
    public                          bool   IsFailure     => !IsSuccess;
    public                          Error  Error         { get; }
    public static                   Result Success()     => SuccessfulResult;
    public static                   Result Failure()     => FailureResult;
    public static implicit operator Result(bool  result) => Success();
    public static implicit operator Result(Error err)    => new(false, err);
}

public readonly record struct Result<TValue>
{
    private readonly TValue? _value;

    private Result(TValue? value, bool isSuccess, Error err)
    {
        switch (isSuccess)
        {
            case true when err  != Error.None: throw new InvalidOperationException();
            case false when err == Error.None: throw new InvalidOperationException();
            default:
                _value    = value;
                IsSuccess = isSuccess;
                Error     = err;
                break;
        }
    }

    public bool  IsSuccess { get; }
    public bool  IsFailure => !IsSuccess;
    public Error Error     { get; }

    public TValue Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException("The value of a failure result cannot be accessed");

    public static implicit operator Result<TValue>(TValue value) => new(value, true, Error.None);
    public static implicit operator Result<TValue>(Error  err)   => new(default, false, err);
}
