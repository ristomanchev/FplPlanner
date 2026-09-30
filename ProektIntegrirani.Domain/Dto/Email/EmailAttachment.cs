namespace ProektIntegrirani.Domain.Dto.Email;

public class EmailAttachment
{
    public const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string? ContentType { get; set; }
}
