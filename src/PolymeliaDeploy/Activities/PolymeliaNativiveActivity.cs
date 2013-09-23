namespace PolymeliaDeploy.Activities
{
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    using PolymeliaDeploy.Data;

    using Variable = System.Activities.Variable;
    using System.Activities.Tracking;
    using System.Diagnostics;
    using System;

    public abstract class PolymeliaNativiveActivity : NativeActivity
    {
        private readonly Collection<Variable> _variables = new Collection<Variable>();

        [Browsable(false)]
        public Collection<Variable> Variables
        {
            get { return _variables; }
        }


        protected override void Execute(NativeActivityContext context)
        {
        }


        protected void ReportInfo(string msg, NativeActivityContext context)
        {
            Report(msg, TraceLevel.Info, context);
        }


        protected void ReportError(string msg, NativeActivityContext context)
        {
            Report(msg, TraceLevel.Error, context);
        }


        protected void ReportWarning(string msg, NativeActivityContext context)
        {
            Report(msg, TraceLevel.Warning, context);
        }


        protected void ReportDebug(string msg, NativeActivityContext context)
        {
            Report(msg, TraceLevel.Verbose, context);
        }


        private void Report(string msg, TraceLevel traceLevel, NativeActivityContext context)
        {
            var activityReport = new ActivityReport
            {
                DeploymentId = PolymeliaActivityContext.Current.DeploymentId,
                MachineName = System.Environment.MachineName,
                ServerRole = PolymeliaActivityContext.Current.ServerRole,
                Environment = PolymeliaActivityContext.Current.Environment,
                ActivityName = DisplayName,
                Message = msg,
                Status = ConvertTraceLevelToReportStatus(traceLevel)
            };

            var customRecord = new CustomTrackingRecord(DisplayName, traceLevel)
            {
                Data = { { "Record", activityReport } }
            };

            context.Track(customRecord);
        }


        private ReportStatus ConvertTraceLevelToReportStatus(TraceLevel traceLevel)
        {
            switch (traceLevel)
            {
                case TraceLevel.Info:
                    return ReportStatus.Information;
                case TraceLevel.Error:
                    return ReportStatus.Error;
                case TraceLevel.Warning:
                    return ReportStatus.Warning;
                default:
                    return ReportStatus.Debug;
            }
        }


        [Obsolete]
        protected void ReportWarning(string msg)
        {
            Report(msg, ReportStatus.Warning);
        }


        [Obsolete]
        protected void ReportDebug(string msg)
        {
            Report(msg,ReportStatus.Debug);
        }

        [Obsolete]

        protected void ReportInfo(string msg)
        {
            Report(msg,ReportStatus.Information);
        }

        [Obsolete]

        protected void ReportError(string msg)
        {
            Report(msg, ReportStatus.Error);
        }

        [Obsolete]

        private void Report(string msg, ReportStatus status)
        {
            if (string.IsNullOrWhiteSpace(msg))
                return;

            //reportRemoteClient.Report(
            //                          PolymeliaActivityContext.Current.DeploymentId,
            //                          PolymeliaActivityContext.Current.ServerRole,
            //                          msg,
            //                          DisplayName,
            //                          status);
        }
    }
}