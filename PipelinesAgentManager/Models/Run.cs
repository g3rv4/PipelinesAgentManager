using System;

namespace PipelinesAgentManager.Models
{
    public class Run
    {
        public string Id { get; set; }
        public RunStatus Status { get; set; }
        public bool IsFinished => Status >= RunStatus.Applied;
        public bool IsErrored => Status == RunStatus.ApplyErrored || Status == RunStatus.PlanErrored;

        public Run() { }

        internal Run(PipelinesAgentManager.Models.Terraform.Run run)
        {
            Id = run.Data.Id;
            if (!Enum.TryParse<RunStatus>(run.Data.Attributes.Status.Replace(" ", ""), ignoreCase: true, out var status))
            {
                throw new ArgumentException("Could not parse run status: " + run.Data.Attributes.Status);
            }
            Status = status;
        }

        public override string ToString() =>
            Jil.JSON.Serialize(this);
    }

    public enum RunStatus
    {
        Unknown = 0,
        Pending = 1,
        Planning = 2,
        NeedsConfirmation = 3,
        CostEstimating = 4,
        CostEstimated = 5,
        PolicyCheck = 6,
        PolicyOverride = 7,
        PolicyChecked = 8,
        Applying = 9,
        Applied = 10,
        NoChanges = 11,
        ApplyErrored = 12,
        PlanErrored = 13,
        Discarded = 14,
        Canceled = 15,
    }
}