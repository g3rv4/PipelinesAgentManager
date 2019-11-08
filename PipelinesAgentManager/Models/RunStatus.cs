using PipelinesAgentManager.Models;

namespace PipelinesAgentManager.Models
{
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

namespace PipelinesAgentManager
{
    public static class RunStatusHelper
    {
        public static bool IsFinished(this RunStatus status) => status >= RunStatus.Applied;
        public static bool IsErrored(this RunStatus status) => status == RunStatus.Errored || status == RunStatus.PolicySoftFailed;
    }
}