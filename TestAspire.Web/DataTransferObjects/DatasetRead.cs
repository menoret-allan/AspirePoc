namespace TestAspire.Web.DataTransferObjects;

public class DatasetRead
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public byte[] Image { get; set; }
}