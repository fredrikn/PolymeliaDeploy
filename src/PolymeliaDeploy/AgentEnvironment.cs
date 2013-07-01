namespace PolymeliaDeploy
{
    using System.Collections.Generic;

    using PolymeliaDeploy.Data;
    using System;


    public static class AgentEnvironment
    {
        [ThreadStatic]
        private static string _serverRole = string.Empty;
        [ThreadStatic]
        private static IDictionary<string, string> _variables;

        [ThreadStatic]
        private static long? _currentActivityId;

        [ThreadStatic]
        private static long _taskId;

        [ThreadStatic]
        private static string _deployVersion;


        public static IDictionary<string, string> Variables
        { 
            get { return _variables; }
            set { _variables = value; }
        }


        public static string ServerRole
        {
            get { return _serverRole; }
            set { _serverRole = value; }
        }


        public static long? CurrentActivityId
        {
            get { return _currentActivityId; }
            set { _currentActivityId = value; }
        }


        public static long TaskId
        {
            get { return _taskId; }
            set { _taskId = value; }
        }


        public static string DeployVersion
        {
            get { return _deployVersion;  }
            set { _deployVersion = value; }
        }
    }
}
