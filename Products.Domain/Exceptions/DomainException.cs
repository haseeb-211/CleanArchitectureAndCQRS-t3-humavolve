namespace Products.Domain.Exceptions;

/// <summary>
/// Raised when a domain invariant is violated.
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException(string message)
        : base(message)
    {
    }
}
