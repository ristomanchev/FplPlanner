namespace ProektIntegrirani.Domain.Exceptions;

// Thrown when a request is well-formed but violates a domain rule
// (duplicate FplId, missing related entity, invalid squad, ...).
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message)
    {
    }
}
