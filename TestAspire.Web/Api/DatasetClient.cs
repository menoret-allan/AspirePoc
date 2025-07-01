using TestAspire.Web.DataTransferObjects;
using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

namespace TestAspire.Web.Api;

public class DatasetClient(HttpClient httpClient)
{
    public async Task<DatasetRead[]> GetDatasetsAsync(int maxItems = 10,
        CancellationToken cancellationToken = default)
    {
        List<DatasetRead> datasets = [];

        await foreach (var dataset in httpClient.GetFromJsonAsAsyncEnumerable<DatasetRead>("/datasets",
                           cancellationToken))
        {
            if (datasets.Count >= maxItems) break;
            if (dataset is not null) datasets.Add(dataset);
        }

        return datasets.ToArray();
    }

    public async Task<HttpResponseMessage> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return await httpClient.DeleteAsync($"datasets/{id}", cancellationToken);
    }

    public async Task<HttpResponseMessage> PostDatasetsAsync(DatasetWrite datasetWrite)
    {
        var formData = new MultipartFormDataContent
        {
            { new StringContent(datasetWrite.Name), nameof(datasetWrite.Name) },
            { new StringContent(datasetWrite.Id.ToString()), nameof(datasetWrite.Id) }
        };

        if (datasetWrite.ImageFile is not null)
        {
            var streamContent = new StreamContent(datasetWrite.ImageFile.OpenReadStream())
            {
                Headers = { ContentType = new MediaTypeHeaderValue(datasetWrite.ImageFile.ContentType) }
            };

            formData.Add(streamContent, "ImageFile", datasetWrite.ImageFile.FileName);
        }

        return await httpClient.PostAsync("/datasets", formData);
    }
}