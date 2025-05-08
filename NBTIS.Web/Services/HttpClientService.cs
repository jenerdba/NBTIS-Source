namespace NBTIS.Web.Services
{
    public class HttpClientService 
    {
        public HttpClient Client { get; }

        public HttpClientService(HttpClient httpClient)
        {
            Client = httpClient;
        }
    }
}
