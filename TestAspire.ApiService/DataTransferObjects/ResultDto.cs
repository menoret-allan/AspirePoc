namespace TestAspire.ApiService.DataTransferObjects
{
    public class ResultDto
    {
        public int Id { get; set; }
        public required DatasetDto Dataset { get; set; }
        public required AlgoDto Algo { get; set; }
        public string? ResultJson { get; set; }
    }
}