using System.Runtime.Serialization;

namespace PipelinesAgentManager.Models.Terraform
{
    internal class CreateRunRequest
    {
        public static CreateRunRequest Create(string workspaceId, string message, bool isDestroy) =>
            new CreateRunRequest
            {
                Data = new CreateRunRequest.DataClass
                {
                    Attributes = new CreateRunRequest.DataClass.AttributesClass
                    {
                        Message = message,
                        IsDestroy = isDestroy
                    },
                    Relationships = new CreateRunRequest.DataClass.RelationshipsClass
                    {
                        Workspace = new CreateRunRequest.DataClass.RelationshipsClass.WorkspaceClass
                        {
                            Data = new CreateRunRequest.DataClass.RelationshipsClass.WorkspaceClass.DataClass
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