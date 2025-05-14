namespace TestAspire.ApiService.Entities;

public class Result
{
    public int Id { get; set; }
    public int DatasetId { get; set; }
    public required Dataset Dataset { get; set; }
    public int AlgoId { get; set; }
    public required Algo Algo { get; set; }
    public string? ResultJson { get; set; }
}