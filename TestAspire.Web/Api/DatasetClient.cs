using TestAspire.Web.DataTransferObjects;
using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

namespace TestAspire.Web.Api;

public class DatasetClient(HttpClient httpClient)
{
    public async Task<DatasetBackend[]> GetDatasetsAsync(int maxItems = 10,
        CancellationToken cancellationToken = default)
    {
        List<DatasetBackend> datasets = [];

        await foreach (var dataset in httpClient.GetFromJsonAsAsyncEnumerable<DatasetBackend>("/datasets",
                           cancellationToken))
        {
            if (datasets.Count >= maxItems) break;
            if (dataset is not null) datasets.Add(dataset);
        }

        return datasets.ToArray();
    }

    public async Task<HttpResponseMessage> PostDatasetsAsync(DatasetDetails datasetDetails)
    {
        var formData = new MultipartFormDataContent
        {
            { new StringContent(datasetDetails.Name), nameof(datasetDetails.Name) },
            { new StringContent(datasetDetails.Id.ToString()), nameof(datasetDetails.Id) }
        };

        if (datasetDetails.ImageFile is not null)
        {
            var streamContent = new StreamContent(datasetDetails.ImageFile.OpenReadStream())
            {
                Headers = { ContentType = new MediaTypeHeaderValue(datasetDetails.ImageFile.ContentType) }
            };

            formData.Add(streamContent, "ImageFile", datasetDetails.ImageFile.FileName);
        }

        return await httpClient.PostAsync("/datasets", formData);
    }
}

public record Dataset(int id, string name, byte[] file)
{
}