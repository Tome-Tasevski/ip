using System.Net.Http;
using System.Threading.Tasks;

namespace ServiceProviderMultiTenant.Services
{
    public interface ISensorDataHttpClient
    {
        Task<HttpClient> GetClientAsync();
    }
}
