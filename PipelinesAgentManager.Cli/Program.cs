using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using PipelinesAgentManager.Cli.Models;

namespace PipelinesAgentManager.Cli
{
    class Program
    {
        private const char PipelinesPoolIdShortName = 'p';
        private const string PipelinesPoolIdLongName = "pool-id";
        private const char TerraformWorkspaceIdShortName = 'w';
        private const string TerraformWorkspaceIdLongName = "workspace-id";
        private const char ConfigFileShortName = 'c';
        private const string ConfigFileLongName = "config-file";


        class OptionsWithFilename
        {
            [Option(ConfigFileShortName, ConfigFileLongName, Required = false, HelpText = "Path to a json config file")]
            public string ConfigFile { get; set; }
        }

        [Verb("create", HelpText = "Creates an environment if needed")]
        class CreateOptions : OptionsWithFilename
        {
            [Option(PipelinesPoolIdShortName, PipelinesPoolIdLongName, Required = true, HelpText = "Azure Pipelines pool ID to check")]
            public int PipelinesPoolId { get; set; }

            [Option(TerraformWorkspaceIdShortName, TerraformWorkspaceIdLongName, Required = true, HelpText = "Terraform Workspace ID to run")]
            public string TerraformWorkspaceId { get; set; }

            [Option('m', "minutes-to-wait", Required = false, HelpText = "Max minutes to wait for Terraform to finish")]
            public int? MinutesToWait { get; set; }
        }

        [Verb("destroy", HelpText = "Creates an environment if needed")]
        class DestroyOptions : OptionsWithFilename
        {
            [Option(PipelinesPoolIdShortName, PipelinesPoolIdLongName, Required = true, HelpText = "Azure Pipelines pool ID to check")]
            public int PipelinesPoolId { get; set; }

            [Option(TerraformWorkspaceIdShortName, TerraformWorkspaceIdLongName, Required = true, HelpText = "Terraform Workspace ID to run")]
            public string TerraformWorkspaceId { get; set; }

            [Option('m', "minutes-without-builds", Required = false, Default = 40, HelpText = "Minutes without builds")]
            public int MinutesWithoutBuilds { get; set; }
            
            [Option('f', "file-to-watch", Required = false, HelpText = "File to watch (in addition to pipelines info) for time to pass")]
            public string FileToWatch { get; set; }
        }

        [Verb("apply", HelpText = "Applies a Terraform run")]
        class ApplyOptions : OptionsWithFilename
        {
            [Option('r', "run-id", Required = true, HelpText = "The run ID")]
            public string RunId { get; set; }
        }

        [Verb("applyIfNeeded", HelpText = "Applies a terraform run if one is awaiting approval")]
        class ApplyIfNeededOptions : OptionsWithFilename
        {
            [Option(TerraformWorkspaceIdShortName, TerraformWorkspaceIdLongName, Required = true, HelpText = "Terraform Workspace ID to check for runs to apply")]
            public string TerraformWorkspaceId { get; set; }
        }

        static Task<int> Main(string[] args)
        {
            args = InitAndTweakArgs(args);
            return CommandLine.Parser.Default.ParseArguments<CreateOptions, DestroyOptions, ApplyOptions, ApplyIfNeededOptions>(args)
              .MapResult(
                (CreateOptions opts) => Create(opts),
                (DestroyOptions opts) => Destroy(opts),
                (ApplyOptions opts) => Apply(opts),
                (ApplyIfNeededOptions opts) => ApplyIfNeeded(opts),
                errs => Task.FromResult(1));
        }

        private static string[] InitAndTweakArgs(string[] args)
        {
            if (args.Length == 0)
            {
                return args;
            }

            var position = Math.Max(Array.IndexOf(args, "-" + ConfigFileShortName), Array.IndexOf(args, "--" + ConfigFileLongName));
            string configFile = null;
            if (position != -1)
            {
                configFile = args[position + 1];
            }

            var pathToFile = configFile ?? Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".PipelinesAgentManager");

            var configFromFileResult = Config.TryCreateFromFile(pathToFile, out var configFromFile);
            var configFromEnv = Config.CreateFromEnvironment();
            Config[] configsToUse;

            if (configFile.HasValue())
            {
                // if there's a file specified, use only that
                if (configFromFileResult != Config.CreateFromFileResult.Success)
                {
                    throw new Exception($"Could not load file {pathToFile}. Result: {configFromFileResult}");
                }
                if (!configFromFile.IsValid)
                {
                    throw new Exception("Config file has missing fields. Ensure the 3 are set.");
                }

                configsToUse = new[] { configFromFile };
            }
            else
            {
                // use the env vars first, and only the file as backup
                configsToUse = new[] { configFromEnv, configFromFile };
            }

            string terraformToken = null, pipelinesPAT = null, pipelinesOrg = null, defaultWorkspaceId = null;
            int? defaultPoolId = null;
            foreach (var config in configsToUse)
            {
                terraformToken ??= config.TerraformToken;
                pipelinesPAT ??= config.PipelinesPAT;
                pipelinesOrg ??= config.PipelinesOrg;
                defaultWorkspaceId ??= config.DefaultWorkspace;
                defaultPoolId ??= config.DefaultPoolId;
            }

            if (terraformToken.IsNullOrEmpty() || pipelinesPAT.IsNullOrEmpty() || pipelinesOrg.IsNullOrEmpty())
            {
                var sb = new StringBuilder("Could not retrieve values for: ");

                void addToSb(string value, string name)
                {
                    if (value.IsNullOrEmpty())
                    {
                        sb.Append(name);
                        sb.Append(", ");
                    }
                }

                addToSb(terraformToken, nameof(terraformToken));
                addToSb(pipelinesPAT, nameof(pipelinesPAT));
                addToSb(pipelinesOrg, nameof(pipelinesOrg));

                sb.Length -= 2;

                throw new Exception(sb.ToString());
            }

            Provisioner.Init(terraformToken, pipelinesPAT, pipelinesOrg);

            if (defaultWorkspaceId.HasValue() || defaultPoolId.HasValue)
            {
                var modifiedArgs = args.ToList();
                var verbType = typeof(Program)
                                    .GetNestedTypes(System.Reflection.BindingFlags.NonPublic)
                                    .Where(t =>
                                    {
                                        var verbAttributes = t.GetCustomAttributes(typeof(VerbAttribute), false);
                                        return verbAttributes.Length > 0 &&
                                                verbAttributes[0] is VerbAttribute va &&
                                                va.Name == args[0];
                                    })
                                    .FirstOrDefault();

                if (verbType != null)
                {
                    var validOptions = verbType
                                        .GetMembers()
                                        .Where(m => m.IsDefined(typeof(OptionAttribute), false))
                                        .Select(m => (m.GetCustomAttributes(typeof(OptionAttribute), false)[0] as OptionAttribute).ShortName)
                                        .ToArray();

                    if (defaultWorkspaceId.HasValue() &&
                        validOptions.Contains(TerraformWorkspaceIdShortName.ToString()) &&
                        !args.Contains("-" + TerraformWorkspaceIdShortName) &&
                        !args.Contains("--" + TerraformWorkspaceIdLongName))
                    {
                        modifiedArgs.Add("-" + TerraformWorkspaceIdShortName);
                        modifiedArgs.Add(defaultWorkspaceId);
                    }

                    if (defaultPoolId.HasValue &&
                        validOptions.Contains(PipelinesPoolIdShortName.ToString()) &&
                        !args.Contains("-" + PipelinesPoolIdShortName) &&
                        !args.Contains("--" + PipelinesPoolIdLongName))
                    {
                        modifiedArgs.Add("-" + PipelinesPoolIdShortName);
                        modifiedArgs.Add(defaultPoolId.ToString());
                    }
                }
                args = modifiedArgs.ToArray();
            }

            return args;
        }

        private static async Task<int> Destroy(DestroyOptions opts)
        {
            var response = await Provisioner.DestroyIfNeededAsync(opts.PipelinesPoolId, opts.TerraformWorkspaceId, opts.MinutesWithoutBuilds, "Destroy from CLI", opts.FileToWatch);
            Console.WriteLine(response);
            return 0;
        }

        private static async Task<int> Create(CreateOptions opts)
        {
            var response = await Provisioner.EnsureThereIsAnAgentAsync(opts.PipelinesPoolId, opts.TerraformWorkspaceId, "Created from CLI");
            Console.WriteLine(response);

            if (response.RunId.HasValue() && opts.MinutesToWait.HasValue)
            {
                Console.WriteLine("Waiting for environment creation...");
                var started = DateTime.UtcNow;
                while ((DateTime.UtcNow - started).TotalMinutes < opts.MinutesToWait.Value)
                {
                    var run = await Provisioner.GetTerraformRunAsync(response.RunId);
                    Console.WriteLine("Status: " + run.Status);

                    if (run.Status.IsFinished())
                    {
                        Console.WriteLine("It has finished!");
                        if (run.Status.IsErrored())
                        {
                            Console.WriteLine("With errors :(");
                        }

                        break;
                    }
                    await Task.Delay(5000);
                }

                // wait for Azure to report the agent as online
                while ((DateTime.UtcNow - started).TotalMinutes < opts.MinutesToWait.Value)
                {
                    var isOnline = await Provisioner.ThereIsAPipelineAgentRunning(opts.PipelinesPoolId);
                    Console.WriteLine("There is an agent online: " + isOnline);
                    if (isOnline)
                    {
                        break;
                    }
                    await Task.Delay(5000);
                }
            }

            return 0;
        }

        private static async Task<int> Apply(ApplyOptions opts)
        {
            var response = await Provisioner.ApplyTerraformRunAsync(opts.RunId);
            Console.WriteLine(response);
            return 0;
        }

        private static async Task<int> ApplyIfNeeded(ApplyIfNeededOptions opts)
        {
            var response = await Provisioner.ApplyTerraformRunIfNeededAsync(opts.TerraformWorkspaceId);
            Console.WriteLine(response);
            return 0;
        }
    }
}
