namespace PolymeliaDeploy.Activities
{
    using System.Activities;
    using System.Collections.Generic;
    using System.ComponentModel;

    public interface IUseDeployVariables
    {
        [Browsable(false)]
        Dictionary<string, object> DeployVariables { get; }

        [Browsable(false)]
        long TaskId { get; set; }

        [Browsable(false)]
        string DeployVersion { get; set; }
    }
}