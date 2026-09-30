namespace FplPlanner.Domain.Dto;

public class ApiClientDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int RequestsPerMinute { get; set; }
}
