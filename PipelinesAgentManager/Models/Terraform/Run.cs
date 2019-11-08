using System;
using System.Runtime.Serialization;

namespace PipelinesAgentManager.Models.Terraform
{
    internal class Run
    {
        public DataClass Data { get; set; }
        public class DataClass
        {
            public string Id { get; set; }
            public AttributesClass Attributes { get; set; }

            public class AttributesClass
            {
                private RunStatus? _Status { get; set; }
                public RunStatus Status => _Status ?? (_Status = ParseStatus(StatusStr)).Value;

                private static RunStatus ParseStatus(string rawStatus)
                {
                    if (!Enum.TryParse<RunStatus>(rawStatus.Replace("_", ""), ignoreCase: true, out var status))
                    {
                        throw new ArgumentException("Could not parse run status: " + rawStatus);
                    }
                    return status;
                }

                [DataMember(Name = "is-destroy")]
                public bool IsDestroy { get; set; }

                [DataMember(Name = "status")]
                public string StatusStr { get; set; }
                public ActionsClass Actions { get; set; }

                public class ActionsClass
                {
                    [DataMember(Name = "is-confirmable")]
                    public bool IsConfirmable { get; set; }
                }
            }
        }
    }

    internal class Runs
    {
        public Run.DataClass[] Data { get; set; }
    }
}