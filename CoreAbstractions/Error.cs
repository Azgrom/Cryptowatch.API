using System;

namespace CoreAbstractions;

public sealed class Error : IEquatable<Error>
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

        if (a is null || b is null)
        {
            return false;
        }

        return a.Code.Equals(b.Code, StringComparison.InvariantCultureIgnoreCase)
               && a.Message.Equals(b.Message, StringComparison.InvariantCultureIgnoreCase);
    }

    public static bool operator !=(Error? a, Error? b)
    {
        return !(a == b);
    }

    public bool Equals(Error? other) => throw new NotImplementedException();

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == this.GetType() && Equals((Error) obj);
    }

    public override int GetHashCode() => throw new NotImplementedException();
}
