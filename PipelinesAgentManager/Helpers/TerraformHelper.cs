using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Jil;
using PipelinesAgentManager.Models.Terraform;

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

        public static async Task<Run> CreateRunAsync(string workspaceId, string message, bool isDestroy)
        {
            var tRequest = CreateRunRequest.Create(workspaceId, message, isDestroy);
            var json = Serialize(tRequest);

            var content = new StringContent(json);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");
            var result = await HttpClient.PostAsync("runs", content);
            result.EnsureSuccessStatusCode();

            json = await result.Content.ReadAsStringAsync();
            return Deserialize<Run>(json);
        }

        public static async Task<string> ApplyRun(string runId)
        {
            var content = new StringContent("");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            var result = await HttpClient.PostAsync($"runs/{runId}/actions/apply", content);
            result.EnsureSuccessStatusCode();
            return await result.Content.ReadAsStringAsync();
        }

        internal static async Task<Run> GetRunAsync(string runId)
        {
            var result = await HttpClient.GetAsync("runs/" + runId);
            result.EnsureSuccessStatusCode();

            var json = await result.Content.ReadAsStringAsync();
            return Deserialize<Run>(json);
        }

        private static string Serialize<T>(T obj) =>
            JSON.Serialize<T>(obj, Options.ISO8601CamelCase);

        private static T Deserialize<T>(string json) =>
            JSON.Deserialize<T>(json, Options.ISO8601CamelCase);
    }
}