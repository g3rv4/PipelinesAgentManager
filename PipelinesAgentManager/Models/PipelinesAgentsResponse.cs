using System;

namespace PipelinesAgentManager.Models
{
    internal class PipelinesAgentsResponse
    {
        public Agent[] Value { get; set; }

        public class Agent
        {
            public string Status { get; set; }
            public DateTime CreatedOn { get; set; }
            public LastCompletedRequestClass LastCompletedRequest { get; set; }

            public class LastCompletedRequestClass
            {
                public DateTime FinishTime { get; set; }
            }
        }
    }
}