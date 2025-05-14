namespace TestAspire.ApiService.Entities;

public class Algo
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Version { get; set; }
    public ICollection<Result> Results { get; set; }
}