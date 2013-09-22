using System.Windows;

namespace PolymeliaDeployClient.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Controls;
    using System.Windows.Media;

    using PolymeliaDeploy.Controller;
    using PolymeliaDeploy.Data;

    using Environment = PolymeliaDeploy.Data.Environment;

    /// <summary>
    /// Interaction logic for Deploy.xaml
    /// </summary>
    public partial class Deploy : Window
    {
        private IActivityClient _client;
        private IReportClient _reportClient;

        private CancellationTokenSource cancellationTokenSource;

        public Environment SelectedEnvironment { get; set; }

        public Deploy()
        {
            InitializeComponent();

            _client = new ActivityRemoteClient();
            _reportClient = new ReportRemoteClient();
        }


        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            environmentTextBox.Text = SelectedEnvironment.Name;
            
            Title = string.Format("Deploy environment '{0}'", SelectedEnvironment.Name);

            var lastestDeploy = _client.LatestDeployedActivity(
                                                              SelectedEnvironment.ProjectId,
                                                              SelectedEnvironment.Id);

            if (lastestDeploy != null)
            {
                verionInfoTextBox.Text = lastestDeploy.Version;
                infoCreatedTextBox.Text = lastestDeploy.Created.ToString();
                statusInfoTextBox.Text = lastestDeploy.Status.ToString();
                createdByInfoTextBox.Text = createdByInfoTextBox.Text;
                versionTextBox.Text = lastestDeploy.Version;
            }

        }


        protected override void OnClosed(EventArgs e)
        {
            if (cancellationTokenSource != null)
                cancellationTokenSource.Cancel();

            base.OnClosed(e);
        }


        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            if (cancellationTokenSource != null)
                cancellationTokenSource.Cancel();
    
            DialogResult = true;
        }


        private void deployButton_Click(object sender, RoutedEventArgs e)
        {
            var deploymentControllerClient = new ActivityRemoteClient();

            var taskId = deploymentControllerClient.Deploy(new MainActivity
            {
                CreatedBy = Thread.CurrentPrincipal.Identity.Name,
                DeployActivity = SelectedEnvironment.WorkflowContent,
                EnvironmentId = SelectedEnvironment.Id,
                Environment = SelectedEnvironment.Name,
                Version = versionTextBox.Text,
                ProjectId = SelectedEnvironment.ProjectId,
            });

            StartGettingReports(taskId);
        }


        private void StartGettingReports(long taskId)
        {
            var rootTreeItem = new TreeViewItem();
            rootTreeItem.Margin = new Thickness(8);

            rootTreeItem.Margin = new Thickness(2);
            rootTreeItem.Header = string.Format("Deploy started '{0}'", DateTime.Now);

            rootTreeItem.ExpandSubtree();

            reportView.Items.Add(rootTreeItem);

            var treeItems = new Dictionary<string, TreeViewItem>();

            cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            Task.Factory.StartNew(
                async Delegate =>
                    {
                        long? latestTaskRunId = 0;

                        while (!cancellationToken.IsCancellationRequested)
                        {
                            var reports = _reportClient.GetReports(taskId, latestTaskRunId).Result;

                            if (reports.Any())
                            {
                                foreach (var activityReport in reports)
                                {
                                    Dispatcher.Invoke(() => AddReport(activityReport, treeItems, rootTreeItem));
                                    latestTaskRunId = activityReport.Id;
                                }
                            }

                            await Task.Delay(2000);
                        }
                    },cancellationToken);
        }


        private static void AddReport(
                                      ActivityReport activityReport,
                                      Dictionary<string, TreeViewItem> treeItems,
                                      TreeViewItem rootTreeItem)
        {
            if (!string.IsNullOrWhiteSpace(activityReport.ServerRole))
                AddReportToServerRole(treeItems, activityReport, rootTreeItem);
            else
            {
                var item = CreateMessageTreeViewItem(activityReport);
                rootTreeItem.Items.Add(item);
            }
        }

        private static void AddReportToServerRole(
                                                  Dictionary<string, TreeViewItem> treeItems, 
                                                  ActivityReport activityReport, 
                                                  TreeViewItem rootTreeItem)
        {
            if (!treeItems.ContainsKey(activityReport.ServerRole))
            {
                var treeItem = new TreeViewItem { Header = activityReport.ServerRole };
                treeItem.Margin = new Thickness(8);
                treeItem.ExpandSubtree();

                rootTreeItem.Items.Add(treeItem);
                treeItems.Add(activityReport.ServerRole, treeItem);
            }

            var serverRoleTreeItem = treeItems[activityReport.ServerRole];

            if (!string.IsNullOrWhiteSpace(activityReport.MachineName))
                AddReportToRolesMachineGroup(activityReport, treeItems, serverRoleTreeItem);
            else
            {
                var item = CreateMessageTreeViewItem(activityReport);
                serverRoleTreeItem.Items.Add(item);
            }
        }


        private static TreeViewItem CreateMessageTreeViewItem(ActivityReport activityReport)
        {
            var item = new TreeViewItem { Header = activityReport.Message };
            item.Margin = new Thickness(0,4,0,4);
            item.ExpandSubtree();

            if (activityReport.Status == ReportStatus.Error)
                item.Foreground = new SolidColorBrush(Color.FromRgb(200,0,0));
            else if (activityReport.Status == ReportStatus.Warning)
                item.Foreground = new SolidColorBrush(Color.FromRgb(200, 100, 0));

            return item;
        }


        private static void AddReportToRolesMachineGroup(
                                                         ActivityReport activityReport,
                                                         Dictionary<string, TreeViewItem> treeItems, 
                                                         TreeViewItem serverRoleTreeItem)
        {
            var roleServerKey = string.Format("{0}/{1}", activityReport.ServerRole, activityReport.MachineName);

            if (!treeItems.ContainsKey(roleServerKey))
            {
                var treeItem = new TreeViewItem { Header = activityReport.MachineName };
                treeItem.Margin = new Thickness(8);
                treeItem.ExpandSubtree();
                serverRoleTreeItem.Items.Add(treeItem);
                treeItems.Add(roleServerKey, treeItem);
            }

            var roleServerItem = treeItems[roleServerKey];

            if (activityReport.ActivityTaskId.HasValue)
                AddReportToRolesMachineNamesActivityGroup(activityReport, treeItems, roleServerItem);
            else
            {
                var item = CreateMessageTreeViewItem(activityReport);
                roleServerItem.Items.Add(item);
            }
        }


        private static void AddReportToRolesMachineNamesActivityGroup(
                                                  ActivityReport activityReport, 
                                                  Dictionary<string, TreeViewItem> treeItems, 
                                                  TreeViewItem roleServerItem)
        {
            var roleServerActivityKey = string.Format(
                                                      "{0}/{1}/{2}", 
                                                      activityReport.ServerRole,
                                                      activityReport.MachineName,
                                                      activityReport.ActivityTaskId);

            if (!treeItems.ContainsKey(roleServerActivityKey))
            {
                var treeItem = new TreeViewItem { Header = activityReport.ActivityName };
                treeItem.Margin = new Thickness(8);
                treeItem.ExpandSubtree();

                roleServerItem.Items.Add(treeItem);
                treeItems.Add(roleServerActivityKey, treeItem);
            }

            var roleServerActivityItem = treeItems[roleServerActivityKey];
            var item = CreateMessageTreeViewItem(activityReport);
            roleServerActivityItem.Items.Add(item);
        }


        private void versionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Version version;
            
            if (Version.TryParse(versionTextBox.Text, out version))
                deployButton.IsEnabled = SelectedEnvironment != null;
            else
                deployButton.IsEnabled = false;
        }
    }
}
