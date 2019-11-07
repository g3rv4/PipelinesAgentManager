using System;

namespace PipelinesAgentManager.Models
{
    public class Run
    {
        public string Id { get; set; }
        public RunStatus Status { get; set; }
        public bool IsFinished => Status >= RunStatus.Applied;
        public bool IsErrored => Status == RunStatus.Errored || Status == RunStatus.PolicySoftFailed;

        public Run() { }

        internal Run(PipelinesAgentManager.Models.Terraform.Run run)
        {
            Id = run.Data.Id;
            if (!Enum.TryParse<RunStatus>(run.Data.Attributes.Status.Replace("_", ""), ignoreCase: true, out var status))
            {
                throw new ArgumentException("Could not parse run status: " + run.Data.Attributes.Status);
            }
            Status = status;
        }

        public override string ToString() =>
            Jil.JSON.Serialize(this);
    }

    // check https://github.com/hashicorp/go-tfe/blob/master/run.go
    public enum RunStatus
    {
        Unknown = 0,
        Pending,
        PlanQueued,
        Planning,
        Planned,
        Confirmed,
        CostEstimating,
        CostEstimated,
        PolicyChecking,
        PolicyOverride,
        PolicyChecked,
        ApplyQueued,
        Applying,

        // finished states run here
        Applied,
        Discarded,
        Canceled,
        PlannedAndFinished,

        // errors start here
        Errored,
        PolicySoftFailed,
    }
}