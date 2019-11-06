using System;
using System.IO;
using Jil;

namespace PipelinesAgentManager.Cli.Models
{
    public class Config
    {
        public const string TerraformTokenEnvVarName = "TERRAFORM_TOKEN";
        public const string PipelinesPATEnvVarName = "PIPELINES_PAT";
        public const string PipelinesOrgEnvVarName = "PIPELINES_ORG";
        public const string DefaultWorkspaceEnvVarName = "DEFAULT_WORKSPACE_ID";
        public const string DefaultPoolIdEnvVarName = "DEFAULT_POOL_ID";

        private static Config _empty;
        public static Config Empty => _empty ?? (_empty = new Config());

        public string TerraformToken { get; private set; }
        public string PipelinesPAT { get; private set; }
        public string PipelinesOrg { get; private set; }
        public string DefaultWorkspace { get; private set; }
        public int? DefaultPoolId { get; private set; }
        public bool IsValid => TerraformToken.HasValue() &&
                               PipelinesPAT.HasValue() &&
                               PipelinesOrg.HasValue();

        public static CreateFromFileResult TryCreateFromFile(string pathToFile, out Config config)
        {
            config = Empty;
            if (File.Exists(pathToFile))
            {
                using (var reader = File.OpenText(pathToFile))
                {
                    try
                    {
                        config = JSON.Deserialize<Config>(reader);
                        return CreateFromFileResult.Success;
                    }
                    catch (DeserializationException)
                    {
                        return CreateFromFileResult.InvalidFile;
                    }
                }
            }

            return CreateFromFileResult.FileNotFound;
        }

        public static Config CreateFromEnvironment() =>
            new Config
            {
                TerraformToken = GetEnvValue(TerraformTokenEnvVarName),
                PipelinesPAT = GetEnvValue(PipelinesPATEnvVarName),
                PipelinesOrg = GetEnvValue(PipelinesOrgEnvVarName),
                DefaultWorkspace = GetEnvValue(DefaultWorkspaceEnvVarName),
                DefaultPoolId = GetEnvValueInt(DefaultPoolIdEnvVarName),
            };

        public enum CreateFromFileResult
        {
            Unknown = 0,
            FileNotFound = 1,
            InvalidFile = 2,
            Success = 3,
        }

        private static string GetEnvValue(string name) =>
            Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);

        private static int? GetEnvValueInt(string name)
        {
            if (int.TryParse(GetEnvValue(name), out var valInt))
            {
                return valInt;
            }
            return null;
        }
    }
}