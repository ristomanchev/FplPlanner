using System.ComponentModel.DataAnnotations;

namespace ProektIntegrirani.Web.Request;

public class TransferAdviceRequest
{
    [Range(1, 8)] public int Horizon { get; set; } = 5;
    [Range(1, 5)] public int MaxTransfers { get; set; } = 2;
}
