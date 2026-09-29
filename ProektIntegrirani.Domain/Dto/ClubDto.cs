namespace ProektIntegrirani.Domain.Dto;

public class ClubDto
{
    public int FplId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public int StrengthHome { get; set; }
    public int StrengthAway { get; set; }
}
