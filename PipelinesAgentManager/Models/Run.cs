using PipelinesAgentManager.Models.Terraform;

namespace PipelinesAgentManager.Models
{
    public class Run
    {
        public string Id { get; set; }
        public RunStatus Status { get; set; }

        public Run() { }

        internal Run(PipelinesAgentManager.Models.Terraform.Run run)
        {
            Id = run.Data.Id;
            Status = run.Data.Attributes.Status;
        }

        public override string ToString() => Jil.JSON.Serialize(this);
    }
}