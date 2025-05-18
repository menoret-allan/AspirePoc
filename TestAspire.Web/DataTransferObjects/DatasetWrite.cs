namespace TestAspire.Web.DataTransferObjects;

public class DatasetWrite
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public IFormFile? ImageFile { get; set; }
    public string? ImageUri { get; set; }
}