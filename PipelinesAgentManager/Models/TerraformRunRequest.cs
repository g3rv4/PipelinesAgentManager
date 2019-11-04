using System.Runtime.Serialization;

namespace PipelinesAgentManager.Models
{
    internal class TerraformRunRequest
    {
        public static TerraformRunRequest Create(string workspaceId, string message, bool isDestroy) =>
            new TerraformRunRequest
            {
                Data = new TerraformRunRequest.DataClass
                {
                    Attributes = new TerraformRunRequest.DataClass.AttributesClass
                    {
                        Message = message,
                        IsDestroy = isDestroy
                    },
                    Relationships = new TerraformRunRequest.DataClass.RelationshipsClass
                    {
                        Workspace = new TerraformRunRequest.DataClass.RelationshipsClass.WorkspaceClass
                        {
                            Data = new TerraformRunRequest.DataClass.RelationshipsClass.WorkspaceClass.DataClass
                            {
                                Id = workspaceId
                            }
                        }
                    }
                }
            };

        public DataClass Data { get; set; }

        public class DataClass
        {
            public AttributesClass Attributes { get; set; }

            public string Type => "runs";

            public RelationshipsClass Relationships { get; set; }

            public class AttributesClass
            {
                [DataMember(Name = "is-destroy")]
                public bool IsDestroy {get;set;}

                public string Message { get; set; }
            }

            public class RelationshipsClass
            {
                public WorkspaceClass Workspace { get; set; }
                public class WorkspaceClass
                {
                    public DataClass Data { get; set; }
                    public class DataClass
                    {
                        public string Type => "workspaces";
                        public string Id { get; set; }
                    }
                }
            }
        }
    }
}