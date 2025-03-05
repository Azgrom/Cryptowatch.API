using System;

namespace CoreAbstractions;

public readonly struct Error : IEquatable<Error>
{
    public static readonly Error None      = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null");

    public Error(string code, string message)
    {
        Code    = code;
        Message = message;
    }

    public string Code    { get; }
    public string Message { get; }

    public static implicit operator string(Error err) => $"{err.Code} -> {err.Message}";

    public static bool operator ==(Error? a, Error? b)
    {
        if (a is null && b is null)
        {
            return true;
        }

        if (a is not null && b is not null)
        {
            return ((Error)a).Equals((Error)b);
        }

        return false;
    }

    public bool Equals(Error  other) => Code.Equals(other.Code, StringComparison.InvariantCultureIgnoreCase)
                                        && Message.Equals(other.Message, StringComparison.InvariantCultureIgnoreCase);

    public static bool operator !=(Error? a, Error? b)
    {
        return !(a == b);
    }

    public bool Equals(Error? other) => throw new NotImplementedException();


    public override bool Equals(object? obj)
    {
        return obj is Error other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Code.GetHashCode() * 397) ^ Message.GetHashCode();
        }
    }
}
