namespace PolymeliaDeploy.Data.Repositories
{
    public interface IDeploymentRepository
    {
        void UpdateDeploymentStatus(long deploymentId, ActivityStatus status);
    }
}