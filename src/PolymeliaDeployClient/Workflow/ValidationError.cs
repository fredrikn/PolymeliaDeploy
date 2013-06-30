using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolymeliaDeployClient.Workflow
{
    using System.Activities.Presentation.Validation;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public class DebugValidationErrorService : IValidationErrorService
    {
        public void ShowValidationErrors(IList<ValidationErrorInfo> errors)
        {
            errors.ToList().ForEach(vei => Debug.WriteLine(string.Format("Error: {0} ", vei.Message)));
        }
    }
}
