namespace TestAspire.AlgorithmDummy.DataTransferObjects;

public class AlgoDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Version { get; set; }
    public bool IsAlive { get; set; }
}