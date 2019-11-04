namespace PipelinesAgentManager.Models
{
    public class EnsureAgentResult
    {
        public bool ThereWasAnAgent { get; set; }
        public string RunId { get; set; }

        public override string ToString() =>
            Jil.JSON.Serialize(this);
    }
}