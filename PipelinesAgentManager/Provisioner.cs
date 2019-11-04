using System;
using System.Threading.Tasks;
using PipelinesAgentManager.Helpers;

namespace PipelinesAgentManager
{
    public static class Provisioner
    {
        private static bool Initted = false;
        public static void Init(string terraformToken, string pipelinesPAT, string pipelinesOrganization)
        {
            PipelinesHelper.Init(pipelinesOrganization, pipelinesPAT);
            TerraformHelper.Init(terraformToken);
            Initted = true;
        }

        public static async Task<string> EnsureThereIsAnAgentAsync(int pipelinesPoolId, string terraformWorkspaceId, string message)
        {
            EnsureInitialization();

            var thereIsAnAgent = await PipelinesHelper.ThereIsARunningAgentAsync(pipelinesPoolId);
            if (!thereIsAnAgent)
            {
                return await TerraformHelper.CreateRunAsync(terraformWorkspaceId, message, isDestroy: false);
            }
            return "No need!";
        }

        public static async Task<string> DestroyIfNeededAsync(int pipelinesPoolId, string terraformWorkspaceId, int minutesWithoutBuilds, string message)
        {
            EnsureInitialization();

            var minutes = await PipelinesHelper.GetMinutesSinceLastActivity(pipelinesPoolId);
            if (!minutes.HasValue)
            {
                return "No agent running";
            }

            if (minutes.Value < minutesWithoutBuilds)
            {
                return $"Last run {minutes} minutes ago. {minutes} < {minutesWithoutBuilds}, I'm bailing";
            }

            return await TerraformHelper.CreateRunAsync(terraformWorkspaceId, message, isDestroy: true);
        }

        public static async Task<string> ApplyTerraformRunAsync(string runId)
        {
            EnsureInitialization();

            return await TerraformHelper.ApplyRun(runId);
        }

        private static void EnsureInitialization()
        {
            if (!Initted)
            {
                throw new Exception("Initialize before use");
            }
        }
    }
}