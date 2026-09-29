using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ProektIntegrirani.Repository.Converters;

// SQLite stores DateTime as text without a time zone, so values come back as Kind=Unspecified.
// All times in this application are UTC; this converter makes that explicit on the way in and out.
public class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
