using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using PipelinesAgentManager.Models.Pipelines;

namespace PipelinesAgentManager.Helpers
{
    internal static class PipelinesHelper
    {
        private static string Organization;
        private static string PAT;
        public static void Init(string organization, string pat)
        {
            Organization = organization;
            PAT = pat;
        }

        private static HttpClient _httpClient;
        private static HttpClient HttpClient
        {
            get
            {
                if (_httpClient == null)
                {
                    _httpClient = new HttpClient()
                    {
                        BaseAddress = new Uri($"https://dev.azure.com/{Organization}/_apis/")
                    };

                    var byteArray = Encoding.ASCII.GetBytes(PAT + ":" + PAT);
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                }
                return _httpClient;
            }
        }

        public static async Task<bool> ThereIsARunningAgentAsync(int poolId)
        {
            var response = await HttpClient.GetAsync($"distributedtask/pools/{poolId}/agents");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var serviceResponse = Deserialize<AgentsResponse>(json);
            return serviceResponse.Value.Any(a => a.Status == "online");
        }

        public static async Task<int?> GetMinutesSinceLastActivity(int poolId)
        {
            var response = await HttpClient.GetAsync($"distributedtask/pools/{poolId}/agents?includeLastCompletedRequest=true");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var serviceResponse = Deserialize<AgentsResponse>(json).Value;

            var finishTimes = serviceResponse.Where(a => a.LastCompletedRequest != null)
                                             .Select(a => a.LastCompletedRequest.FinishTime);
            var dates = serviceResponse.Select(a => a.CreatedOn)
                                       .Concat(finishTimes);

            return dates.Any()
                ? (int)(DateTime.UtcNow - dates.Max()).TotalMinutes
                : (int?)null;
        }

        private static T Deserialize<T>(string json) =>
            Jil.JSON.Deserialize<T>(json, Jil.Options.ISO8601CamelCase);
    }
}