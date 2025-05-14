namespace TestAspire.Web.Api;

public class AlgoClient(HttpClient httpClient)
{
    public async Task<Algo[]> GetAlgosAsync(int maxItems = 10, CancellationToken cancellationToken = default)
    {
        List<Algo> algos = [];

        await foreach (var algo in httpClient.GetFromJsonAsAsyncEnumerable<Algo>("/algos",
                           cancellationToken))
        {
            if (algos.Count >= maxItems) break;
            if (algo is not null) algos.Add(algo);
        }

        return algos.ToArray();
    }
}

public record Algo(int id, string name, string version)
{
}