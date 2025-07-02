namespace TestAspire.Web.DataTransferObjects;

public class DatasetWrite
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public IFormFile? ImageFile { get; set; }
    public string? ImageUri { get; set; }
}

public class AlgoWrite
{
    public const bool Alive = false;
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Version { get; set; }
}