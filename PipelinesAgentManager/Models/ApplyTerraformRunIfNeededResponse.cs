using System.Collections.Generic;

namespace PipelinesAgentManager.Models
{
    public class ApplyTerraformRunIfNeededResponse
    {
        public List<string> RunsApplied { get; set; }

        public override string ToString() => Jil.JSON.Serialize(this);

        public ApplyTerraformRunIfNeededResponse()
        {
            RunsApplied = new List<string>();
        }
    }
}