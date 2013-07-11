namespace PolymeliaDeploy
{
    using System;
    using System.Collections.ObjectModel;

    using PolymeliaDeploy.Data;

    public class PolymeliaActivityContext
    {
        private readonly static object SyncRoot = new object();

        [ThreadStatic]
        private static volatile PolymeliaActivityContext _current;

        public static PolymeliaActivityContext Current
        {
            get
            {
                if (_current == null)
                {
                    lock (SyncRoot)
                    {
                        if (_current == null)
                            _current = new PolymeliaActivityContext();
                    }
                }

                return _current;
            }
        }

        public Collection<DeployVariable> Variables { get; set; }
        public string ServerRole { get; set; }
        public long? CurrentActivityId { get; set; }
        public long TaskId { get; set; }
        public string DeployVersion { get; set; }

        public string Environment { get; set; }
    }
}
