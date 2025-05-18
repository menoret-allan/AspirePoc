namespace TestAspire.Web.DataTransferObjects;

public class DatasetDetails
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public IFormFile? ImageFile { get; set; }
    public string? ImageUri { get; set; }
}

public class DatasetBackend
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public byte[] Image { get; set; }
}