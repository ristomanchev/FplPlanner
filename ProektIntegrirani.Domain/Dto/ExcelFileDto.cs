namespace ProektIntegrirani.Domain.Dto;

public class ExcelFileDto
{
    public const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = [];
}
