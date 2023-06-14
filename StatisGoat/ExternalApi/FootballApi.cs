using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace StatisGoat.ExternalApi
{
    public class FootballApi : IFootballApi
    {
        readonly IConfiguration configuration;

        public FootballApi(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task<HttpResponseMessage> GetAsync(string path)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://api-football-v1.p.rapidapi.com/v3/{path}"),
                Headers =
                {
                    { "X-RapidAPI-Key", configuration["FootballApiKey"] },
                    { "X-RapidAPI-Host", "api-football-v1.p.rapidapi.com" },
                },
            };

            return await client.SendAsync(request);
        }
    }
}