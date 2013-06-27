namespace PolymeliaDeploy.Process
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class ProcessArgument
    {
        private ICollection<string> _commandArgs = new Collection<string>();

        public string CommandToRun { get; set; }

        public string WorkingDirectory { get; set; }

        public int? MaxWaitTime { get; set; }

        public ICollection<string> CommandArguments
        {
            get { return this._commandArgs; }
            set { this._commandArgs = value; }
        }
    }
}
