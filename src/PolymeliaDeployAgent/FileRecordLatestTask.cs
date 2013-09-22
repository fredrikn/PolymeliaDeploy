using System.Globalization;
using System.IO;

namespace PolymeliaDeploy.Agent
{
    public class FileRecordLatestTask : IRecordLatestTask
    {
        private readonly string _fileName;
        private long? _latestTask;

        public FileRecordLatestTask(string fileName)
        {
            _fileName = fileName;
        }

        public string FileName
        {
            get { return _fileName; }
        }

        public void Refresh()
        {
            if (!File.Exists(_fileName))
                return;

            var taskId = File.ReadAllText(_fileName);

            long result;
            if (long.TryParse(taskId, out result))
                _latestTask = result;
        }

        public long GetValue()
        {
            if (!_latestTask.HasValue) Refresh();
            if (!_latestTask.HasValue) return 0;
            return _latestTask.Value;
        }

        public void SetValue(long value)
        {
            File.WriteAllText(_fileName, value.ToString(CultureInfo.InvariantCulture));
            _latestTask = value;
        }
    }
}