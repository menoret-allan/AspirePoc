using TestAspire.Web.Api;

namespace TestAspire.Web.DataTransferObjects;

public class ResultRead
{
    public int Id { get; set; }
    public required DatasetRead Dataset { get; set; }
    public required Algo Algo { get; set; }
    public string? ResultJson { get; set; }
}

public record ResultWrite(int AlgoId, int DatasetId)
{
    public int AlgoId { get; set; } = AlgoId;
    public int DatasetId { get; set; } = DatasetId;
}