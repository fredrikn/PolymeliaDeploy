using PolymeliaDeploy.ApiDto;
using PolymeliaDeploy.Data;
using System;
using System.Collections.Generic;
namespace PolymeliaDeploy.Agent.Activity
{
    public interface ITaskActivityExecutioner
    {
        long? ExecuteTasks(
                          IEnumerable<ActivityTaskDto> tasks,
                          Action<ActivityReport> reportAction = null,
                          Action<ActivityTaskDto> activitySucceededAction = null,
                          Action<ActivityTaskDto, Exception> activityFailedAction = null);
    }
}