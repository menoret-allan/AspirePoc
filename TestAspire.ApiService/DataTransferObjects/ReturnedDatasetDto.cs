namespace TestAspire.ApiService.DataTransferObjects;

public class ReturnDatasetDto
{
    public int Id { get; set; }
    public required byte[] Image { get; set; }
    public required string Name { get; set; }
}