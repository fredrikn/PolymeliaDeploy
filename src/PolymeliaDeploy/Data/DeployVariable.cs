namespace PolymeliaDeploy.Data
{
    public class DeployVariable
    {
        public long Id { get; set; }

        public long TaskId { get; set; }

        public string VariableKey { get; set; }

        public string VariableValue { get; set; }

        public string Scope { get; set; }
    }
}
