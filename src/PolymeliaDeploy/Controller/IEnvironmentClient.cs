namespace PolymeliaDeploy.Controller
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using PolymeliaDeploy.Data;

    public interface IEnvironmentClient
    {
        Task<IEnumerable<Environment>> GetProjectsEnvironments(int projectId);

        Task<Environment> GetEnvironment(int environmentId);

        void UpdateEnvironment(Environment environment);

        Environment AddEnvironment(Environment environment);

        void DeleteEnvironment(Environment environment);
    }
}