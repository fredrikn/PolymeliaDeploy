using PolymeliaDeploy.Data;

using System;
using System.Activities.Tracking;

namespace PolymeliaDeploy.Workflow
{
    using PolymeliaDeploy.DeployController;

    public class PolymeliaTrackingParticipant : TrackingParticipant
    {
        private IDeployConrollerReportClient _reportClient;

        public PolymeliaTrackingParticipant(IDeployConrollerReportClient reportClient)
        {
            if (reportClient == null) throw new ArgumentNullException("reportClient");

            _reportClient = reportClient;

        }


        protected override void Track(TrackingRecord record, TimeSpan timeout)
        {
            var customTrackingRecord = record as CustomTrackingRecord;

            if ((customTrackingRecord != null) && (customTrackingRecord.Data.Count > 0))
            {
                if (customTrackingRecord.Data.ContainsKey("Record"))
                {
                    var activityReport = customTrackingRecord.Data["Record"] as ActivityReport;

                    if (activityReport != null)
                    {
                        Console.WriteLine(activityReport.Message);
                        _reportClient.Report(activityReport);
                    }
                }
            }
        }
    }
}