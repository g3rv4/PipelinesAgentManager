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
            }
        }
    }
}