namespace FplPlanner.Domain.Exceptions;

// Thrown when a request is well-formed but violates a domain rule
// (duplicate FplId, missing related entity, invalid squad, ...).
public class BusinessRuleException : Exception
{
    // Individual violations when there are several (e.g. every broken squad rule or bad Excel row).
    public IReadOnlyList<string> Errors { get; }

    public BusinessRuleException(string message) : this(message, [])
    {
    }

    public BusinessRuleException(string message, IReadOnlyList<string> errors) : base(message)
    {
        Errors = errors;
    }
}
