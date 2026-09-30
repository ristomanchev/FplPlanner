using System.Security.Cryptography;
using System.Text;

namespace ProektIntegrirani.Domain.ExternalModels;

// Deterministic Guid from an external id: the same FPL player always gets the same Id,
// so the ETL can upsert by primary key and set foreign keys without looking anything up.
public static class GuidHelper
{
    public static Guid FromExternalId(string entityType, int externalId)
    {
        var input = $"{entityType}:{externalId}";
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}
