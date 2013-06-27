namespace PolymeliaDeploy.Controller
{
    using System.Collections.Generic;

    using PolymeliaDeploy.Data;

    public interface IVariableClient
    {
        void AddVariables(IEnumerable<Variable> variables, int environmentId);

        IEnumerable<Variable> GetAllVariables(int environmentId);
    }
}