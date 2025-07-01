using TestAspire.Web.DataTransferObjects;

namespace TestAspire.Web.Api;

public class ResultClient(HttpClient httpClient)
{
    public async Task<ResultRead[]> GetResultsAsync(int maxItems = 10,
        CancellationToken cancellationToken = default)
    {
        List<ResultRead> results = [];

        await foreach (var result in httpClient.GetFromJsonAsAsyncEnumerable<ResultRead>("/results",
                           cancellationToken))
        {
            if (results.Count >= maxItems) break;
            if (result is not null) results.Add(result);
        }

        return results.ToArray();
    }

    public async Task<HttpResponseMessage> PostResultsAsync(ResultWrite resultWrite)
    {
        return await httpClient.PostAsJsonAsync("/results", resultWrite);
    }

    public async Task<HttpResponseMessage> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return await httpClient.DeleteAsync($"results/{id}", cancellationToken);
    }
}