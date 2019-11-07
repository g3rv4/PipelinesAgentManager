namespace PipelinesAgentManager.Models
{
    public class DestroyResult
    {
        public int? Minutes { get; set; }
        public bool ThereWasAnAgent => Minutes.HasValue;
        public string RunId { get; set; }

        public override string ToString() => Jil.JSON.Serialize(this);
    }
}