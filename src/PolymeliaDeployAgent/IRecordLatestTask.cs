namespace PolymeliaDeploy.Agent
{
    public interface IRecordLatestTask
    {
        long GetValue();
        void SetValue(long value);
    }
}