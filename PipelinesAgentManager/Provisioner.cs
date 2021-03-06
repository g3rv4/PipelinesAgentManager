using System;
using System.IO;
using System.Threading.Tasks;
using PipelinesAgentManager.Helpers;
using PipelinesAgentManager.Models;

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

        public static async Task<EnsureAgentResult> EnsureThereIsAnAgentAsync(int pipelinesPoolId, string terraformWorkspaceId, string message)
        {
            EnsureInitialization();
            var result = new EnsureAgentResult();

            result.ThereWasAnUnfinishedApply = await TerraformHelper.ThereIsAnUnfinishedRun(terraformWorkspaceId, isDestroy: false);
            result.ThereWasAnAgent = await PipelinesHelper.ThereIsARunningAgentAsync(pipelinesPoolId);
            if (result.ThereWasAnUnfinishedApply || result.ThereWasAnAgent)
            {
                return result;
            }

            var tfResponse = await TerraformHelper.CreateRunAsync(terraformWorkspaceId, message, isDestroy: false);
            result.RunId = tfResponse.Data.Id;
            return result;
        }

        public static async Task<DestroyResult> DestroyIfNeededAsync(int pipelinesPoolId, string terraformWorkspaceId, int minutesWithoutBuilds, string message, string fileToCheck = null)
        {
            EnsureInitialization();

            var result = new DestroyResult();

            result.ThereWasAnUnfinishedDestroy = await TerraformHelper.ThereIsAnUnfinishedRun(terraformWorkspaceId, isDestroy: true);

            var fileMinutes = int.MaxValue;
            if (fileToCheck.HasValue() && File.Exists(fileToCheck))
            {
                fileMinutes = (int)DateTime.UtcNow.Subtract(new FileInfo(fileToCheck).LastWriteTimeUtc).TotalMinutes;
            }
            result.Minutes = Math.Min(fileMinutes, await PipelinesHelper.GetMinutesSinceLastActivity(pipelinesPoolId));
            if (result.ThereWasAnUnfinishedDestroy || !result.ThereWasAnAgent || result.Minutes.Value < minutesWithoutBuilds)
            {
                return result;
            }

            var tfResponse = await TerraformHelper.CreateRunAsync(terraformWorkspaceId, message, isDestroy: true);
            result.RunId = tfResponse.Data.Id;
            return result;
        }

        public static async Task<Run> GetTerraformRunAsync(string runId)
        {
            EnsureInitialization();

            var run = await TerraformHelper.GetRunAsync(runId);
            return new Run(run);
        }

        public static async Task<string> ApplyTerraformRunAsync(string runId)
        {
            EnsureInitialization();

            return await TerraformHelper.ApplyRunAsync(runId);
        }

        public static async Task<ApplyTerraformRunIfNeededResponse> ApplyTerraformRunIfNeededAsync(string terraformWorkspaceId)
        {
            EnsureInitialization();

            return await TerraformHelper.ApplyRunIfNeededAsync(terraformWorkspaceId);
        }

        public static async Task<bool> ThereIsAPipelineAgentRunning(int pipelinesPoolId)
        {
            EnsureInitialization();

            return await PipelinesHelper.ThereIsARunningAgentAsync(pipelinesPoolId);
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