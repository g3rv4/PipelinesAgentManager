using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using PipelinesAgentManager.Models;

namespace PipelinesAgentManager.Helpers
{
    internal static class TerraformHelper
    {
        private static string Token;

        public static void Init(string token)
        {
            Token = token;
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
                        BaseAddress = new Uri("https://app.terraform.io/api/v2/")
                    };

                    _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + Token);
                }
                return _httpClient;
            }
        }

        public static async Task<string> CreateRunAsync(string workspaceId, string message, bool isDestroy)
        {
            var tRequest = TerraformRunRequest.Create(workspaceId, message, isDestroy);
            var json = Serialize(tRequest);

            var content = new StringContent(json);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");
            var result = await HttpClient.PostAsync("runs", content);
            result.EnsureSuccessStatusCode();
            return await result.Content.ReadAsStringAsync();
        }

        public static async Task<string> ApplyRun(string runId)
        {
            var content = new StringContent("");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            var result = await HttpClient.PostAsync($"runs/{runId}/actions/apply", content);
            result.EnsureSuccessStatusCode();
            return await result.Content.ReadAsStringAsync();
        }

        private static string Serialize<T>(T obj) =>
            Jil.JSON.Serialize<T>(obj, Jil.Options.ISO8601CamelCase);
    }
}