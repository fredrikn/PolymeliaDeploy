namespace PolymeliaDeploy.Data
{
    public class Variable
    {
        public int Id { get; set; }

        public int EnvironmentId { get; set; }

        public string VariableKey { get; set; }

        public string VariableValue { get; set; }

        public string Scope { get; set; }
    }
}
