namespace TestAspire.ApiService.DataTransferObjects;

public class ResultDto
{
    public int Id { get; set; }
    public required DatasetReadDto Dataset { get; set; }
    public required AlgoDto Algo { get; set; }
    public string? ResultJson { get; set; }
}

public class ResultWriteDto
{
    public int DatasetId { get; set; }
    public int AlgoId { get; set; }
}