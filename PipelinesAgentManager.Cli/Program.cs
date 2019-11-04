using System;
using System.Threading.Tasks;
using CommandLine;

namespace PipelinesAgentManager.Cli
{
    class Program
    {
        [Verb("create", HelpText = "Creates an environment if needed")]
        class CreateOptions
        {
            [Option('p', "poolId", Required = true, HelpText = "Azure Pipelines pool ID to check")]
            public int PipelinesPoolId { get; set; }

            [Option('w', "workspaceId", Required = true, HelpText = "Terraform Workspace ID to run")]
            public string TerraformWorkspaceId { get; set; }
        }

        [Verb("destroy", HelpText = "Creates an environment if needed")]
        class DestroyOptions
        {
            [Option('p', "poolId", Required = true, HelpText = "Azure Pipelines pool ID to check")]
            public int PipelinesPoolId { get; set; }

            [Option('w', "workspaceId", Required = true, HelpText = "Terraform Workspace ID to run")]
            public string TerraformWorkspaceId { get; set; }

            [Option('m', "minutesWithoutBuilds", Required = false, Default = 40, HelpText = "Minutes without builds")]
            public int MinutesWithoutBuilds { get; set; }
        }

        [Verb("apply", HelpText = "Applies a Terraform run")]
        class ApplyOptions
        {
            [Option('r', "runId", Required = true, HelpText = "The run ID")]
            public string RunId { get; set; }
        }

        static Task<int> Main(string[] args)
        {
            Init();

            return CommandLine.Parser.Default.ParseArguments<CreateOptions, DestroyOptions, ApplyOptions>(args)
              .MapResult(
                (CreateOptions opts) => Create(opts),
                (DestroyOptions opts) => Destroy(opts),
                (ApplyOptions opts) => Apply(opts),
                errs => Task.FromResult(1));
        }

        private static void Init()
        {
            var terraformToken = Environment.GetEnvironmentVariable("TERRAFORM_TOKEN", EnvironmentVariableTarget.Process);
            var pipelinesPAT = Environment.GetEnvironmentVariable("PIPELINES_PAT", EnvironmentVariableTarget.Process);
            var pipelinesOrganization = Environment.GetEnvironmentVariable("PIPELINES_ORG", EnvironmentVariableTarget.Process);

            if (terraformToken.IsNullOrEmpty() || pipelinesPAT.IsNullOrEmpty() || pipelinesOrganization.IsNullOrEmpty())
            {
                throw new Exception("Define environment variables TERRAFORM_TOKEN, PIPELINES_PAT and PIPELINES_ORG");
            }

            Provisioner.Init(terraformToken, pipelinesPAT, pipelinesOrganization);
        }

        private static async Task<int> Destroy(DestroyOptions opts)
        {
            var response = await Provisioner.DestroyIfNeededAsync(opts.PipelinesPoolId, opts.TerraformWorkspaceId, opts.MinutesWithoutBuilds, "Destroy from CLI");
            Console.WriteLine(response);
            return 0;
        }

        private static async Task<int> Create(CreateOptions opts)
        {
            var response = await Provisioner.EnsureThereIsAnAgentAsync(opts.PipelinesPoolId, opts.TerraformWorkspaceId, "Created from CLI");
            Console.WriteLine(response);
            return 0;
        }

        private static async Task<int> Apply(ApplyOptions opts)
        {
            var response = await Provisioner.ApplyTerraformRunAsync(opts.RunId);
            Console.WriteLine(response);
            return 0;
        }
    }
}
