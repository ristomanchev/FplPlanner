using System.Net.Mail;

namespace FplPlanner.Service.Logic;

public static class EmailAddressValidator
{
    // A plain address (no display name) with a domain that has a dot: "ana@example.com", not "ana@example".
    public static bool IsValid(string? email)
    {
        if (string.IsNullOrWhiteSpace(email) || email != email.Trim() || email.Length > 254)
        {
            return false;
        }

        return MailAddress.TryCreate(email, out var address)
               && address.Address == email
               && address.Host.Contains('.')
               && !address.Host.StartsWith('.')
               && !address.Host.EndsWith('.');
    }
}
