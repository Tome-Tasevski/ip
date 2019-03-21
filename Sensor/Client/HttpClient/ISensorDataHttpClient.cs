using System.Net.Http;
using System.Threading.Tasks;

namespace Client.Services
{
    public interface ISensorDataHttpClient
    {
        Task<HttpClient> GetClientAsync();
    }
}
