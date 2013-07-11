using System;

namespace PolymeliaDeploy.Workflow
{
    using System.Activities.Tracking;

    using PolymeliaDeploy.DeployController;

    public class PolymeliaTrackingParticipantFactory
    {
        public PolymeliaTrackingParticipant CreateTrackingParticipant(IDeployConrollerReportClient reportClient)
        {
            const String ALL = "*";

            return new PolymeliaTrackingParticipant(reportClient)
            {
                TrackingProfile = new TrackingProfile
                {
                    Name = "CustomTrackingProfile",
                    Queries = 
                    {
                        new CustomTrackingQuery { Name = ALL, ActivityName = ALL }
                    }
                }
            };
        }
    }
}
