namespace PolymeliaDeploy.Controller
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using PolymeliaDeploy.Data;

    public interface IProjectClient
    {
        Task<IEnumerable<Project>> GetAllProjects();

        Project CreateProject(Project project, int? copyFromProjectId = null);

        void DeleteProject(Project project);
    }
}