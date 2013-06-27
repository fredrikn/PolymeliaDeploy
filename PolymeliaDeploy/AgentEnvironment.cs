namespace PolymeliaDeploy
{
    using System.Collections.Generic;

    using PolymeliaDeploy.Data;

    public static class AgentEnvironment
    {
        private static string _serverRole = string.Empty;


        public static IEnumerable<Variable> Variables { get; internal set; }


        public static string ServerRole
        {
            get { return _serverRole; }
            set { _serverRole = value; }
        }

        public static long? CurrentActivityId { get; set; }
    }
}
