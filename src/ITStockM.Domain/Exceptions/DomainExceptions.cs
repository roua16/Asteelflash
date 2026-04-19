namespace ITStockM.Domain.Exceptions;

/// <summary>
/// Base exception for domain layer.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}

/// <summary>
/// Thrown when a domain entity is not found.
/// </summary>
public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, object key)
        : base($"{entityName} with identifier {key} was not found.")
    {
    }
}

/// <summary>
/// Thrown when a business rule is violated.
/// </summary>
public class BusinessRuleViolationException : DomainException
{
    public BusinessRuleViolationException(string message) : base(message) { }
}

/// <summary>
/// Thrown when an operation is invalid for the current entity state.
/// </summary>
public class InvalidOperationException : DomainException
{
    public InvalidOperationException(string message) : base(message) { }
}
