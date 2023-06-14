using System.Net.Http;
using System.Threading.Tasks;

namespace StatisGoat.ExternalApi
{
    public interface IFootballApi
    {
        Task<HttpResponseMessage> GetAsync(string path);
    }
}