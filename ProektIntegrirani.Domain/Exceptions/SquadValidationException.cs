namespace ProektIntegrirani.Domain.Exceptions;

// A squad broke one or more FPL rules; every violation is listed, not only the first.
public class SquadValidationException : BusinessRuleException
{
    public IReadOnlyList<string> Errors { get; }

    public SquadValidationException(IReadOnlyList<string> errors)
        : base("The squad breaks FPL rules.")
    {
        Errors = errors;
    }
}
