namespace PolymeliaDeploy.Data
{
    public class DeployVariable
    {
        public long Id { get; set; }

        public long DeploymentId { get; set; }

        public string VariableKey { get; set; }

        public string VariableValue { get; set; }

        public string Scope { get; set; }
    }
}
