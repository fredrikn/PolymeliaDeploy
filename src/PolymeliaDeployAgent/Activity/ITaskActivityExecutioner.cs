using PolymeliaDeploy.ApiDto;

using System;
using System.Collections.Generic;
namespace PolymeliaDeploy.Agent.Activity
{
    public interface ITaskActivityExecutioner
    {
        long? ExecuteTasks(
                          IEnumerable<ActivityTaskDto> tasks,
                          Action<ActivityTaskDto> activitySucceededAction = null,
                          Action<ActivityTaskDto, string> activityFailedAction = null);
    }
}