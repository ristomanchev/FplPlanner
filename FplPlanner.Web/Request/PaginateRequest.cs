namespace FplPlanner.Web.Request;

public class PaginateRequest
{
    public const int MaxPageSize = 100;
    private int _pageSize = 20;

    public int PageNumber { get; set; } = 0;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value is > MaxPageSize or <= 0 ? MaxPageSize : value;
    }
}
