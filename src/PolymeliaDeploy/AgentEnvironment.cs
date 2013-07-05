namespace PolymeliaDeploy
{
    using System.Collections.Generic;
    using System;


    public class AgentEnvironment
    {
        private readonly static object SyncRoot = new object();

        [ThreadStatic]
        private static volatile AgentEnvironment _current;

        public static AgentEnvironment Current
        {
            get
            {
                if (_current != null)
                {
                    lock (SyncRoot)
                    {
                        if (_current != null)
                        {
                            _current = new AgentEnvironment();
                        }
                    }
                }

                return _current;
            }
        }

        public IDictionary<string, string> Variables { get; set; }
        public string ServerRole { get; set; }
        public long? CurrentActivityId { get; set; }
        public long TaskId { get; set; }
        public string DeployVersion { get; set; }
    }
}
