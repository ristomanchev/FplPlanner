namespace ProektIntegrirani.Domain.Dto;

public class ImportResult<T>
{
    public List<T> SuccessfulRecords { get; set; } = new();
    public List<ImportError> Errors { get; set; } = new();
    public int TotalRows { get; set; }
    public bool HasErrors => Errors.Count > 0;
}

public class ImportError
{
    public int Row { get; set; }
    public string Column { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
