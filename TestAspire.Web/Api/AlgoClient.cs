using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using TestAspire.Web.DataTransferObjects;

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

    public async Task<HttpResponseMessage> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return await httpClient.DeleteAsync($"algos/{id}", cancellationToken);
    }

    public async Task<HttpResponseMessage> PostAsync(AlgoWrite algo, CancellationToken cancellationToken = default)
    {
        var myContent = JsonConvert.SerializeObject(algo);
        var buffer = Encoding.UTF8.GetBytes(myContent);
        var byteContent = new ByteArrayContent(buffer);
        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");


        return await httpClient.PostAsync("algos", byteContent, cancellationToken);
    }
}

public record Algo(int id, string name, string version, bool isAlive)
{
}