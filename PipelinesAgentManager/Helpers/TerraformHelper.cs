using System;
using System.Linq;
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

        public static async Task<string> ApplyRunAsync(string runId)
        {
            var content = new StringContent("");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            var result = await HttpClient.PostAsync($"runs/{runId}/actions/apply", content);
            result.EnsureSuccessStatusCode();
            return await result.Content.ReadAsStringAsync();
        }

        private static async Task<Runs> GetRunsInWorkspace(string workspaceId)
        {
            var result = await HttpClient.GetAsync($"workspaces/{workspaceId}/runs");
            result.EnsureSuccessStatusCode();

            var json = await result.Content.ReadAsStringAsync();
            return Deserialize<Runs>(json);
        }

        public static async Task<Models.ApplyTerraformRunIfNeededResponse> ApplyRunIfNeededAsync(string workspaceId)
        {
            var runs = await GetRunsInWorkspace(workspaceId);

            var res = new Models.ApplyTerraformRunIfNeededResponse();
            foreach (var run in runs.Data.Where(r => r.Attributes.Actions.IsConfirmable))
            {
                await ApplyRunAsync(run.Id);
                res.RunsApplied.Add(run.Id);
            }

            return res;
        }

        public static async Task<bool> ThereIsAnUnfinishedRun(string workspaceId, bool isDestroy)
        {
            var runs = await GetRunsInWorkspace(workspaceId);
            return runs.Data.Any(r => !r.Attributes.Status.IsFinished() && r.Attributes.IsDestroy == isDestroy);
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