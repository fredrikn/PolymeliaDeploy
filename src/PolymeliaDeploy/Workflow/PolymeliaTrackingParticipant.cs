using PolymeliaDeploy.Data;
using PolymeliaDeploy.DeployController;
using System;
using System.Activities.Tracking;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolymeliaDeploy.Workflow
{
    public class PolymeliaTrackingParticipant : TrackingParticipant
    {
        private Action<ActivityReport> _reportAction;

        public PolymeliaTrackingParticipant(Action<ActivityReport> reportAction)
        {
            _reportAction = reportAction;
        }


        protected override void Track(TrackingRecord record, TimeSpan timeout)
        {
            var customTrackingRecord = record as CustomTrackingRecord;

            if ((customTrackingRecord != null) && (customTrackingRecord.Data.Count > 0))
            {
                if (customTrackingRecord.Data.ContainsKey("Record"))
                {
                    var activityReport = customTrackingRecord.Data["Record"] as ActivityReport;

                    if (activityReport != null && _reportAction != null)
                    {
                        Console.WriteLine(activityReport.Message);
                        _reportAction(activityReport);
                    }
                }
            }
        }
    }
}