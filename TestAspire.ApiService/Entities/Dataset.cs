namespace TestAspire.ApiService.Entities;

public class Dataset
{
    public int Id { get; set; }
    public required string Image { get; set; }
    public required string Name { get; set; }
    public ICollection<Result> Results { get; set; }
}