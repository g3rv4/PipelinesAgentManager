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
                public string Status { get; set; }
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