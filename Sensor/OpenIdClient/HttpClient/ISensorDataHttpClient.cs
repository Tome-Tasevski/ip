using System.Net.Http;
using System.Threading.Tasks;

namespace OpenIdClient.Services
{
    public interface ISensorDataHttpClient
    {
        Task<HttpClient> GetClientAsync();
    }
}
