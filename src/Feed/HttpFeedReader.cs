using System.IO;
using System.Net.Http;

namespace ArtemisBlog.Feed
{
    class HttpFeedReader
    {
        private readonly string url;

        internal HttpFeedReader(string url)
        {
            this.url = url;
        }

        private HttpClient GetClient()
        {
            HttpClient client = new();
            client.DefaultRequestHeaders.Add("User-Agent", $"{Vsix.Name}/{Vsix.Version} ({Vsix.Name} for Visual Studio)");

            return client;
        }

        internal async Task<Stream> GetFeedStreamAsync()
        {
            using HttpClient client = GetClient();
            using HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            Stream feedData = await response.Content.ReadAsStreamAsync();
            MemoryStream result = new();
            await feedData.CopyToAsync(result);
            return result;
        }

        internal async Task<DateTime> GetLastModifiedAsync()
        {
#if (DEBUG)
            await Task.Delay(5000);
#endif
            using HttpClient client = GetClient();
            using HttpRequestMessage request = new(HttpMethod.Head, url);
            using HttpResponseMessage response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            if (response.Content.Headers.LastModified.HasValue)
            {
                return response.Content.Headers.LastModified.Value.ToLocalTime().DateTime;
            }

            return DateTime.MinValue;
        }
    }
}
