using System.Windows;

namespace PolymeliaDeployClient.Forms
{
    using System.Threading.Tasks;
    using System.Windows.Media;

    using PolymeliaDeploy.Controller;

    /// <summary>
    /// Interaction logic for SelectAgents.xaml
    /// </summary>
    public partial class SelectAgents : Window
    {
        public SelectAgents()
        {
            InitializeComponent();

            var agentClient = new AgentRemoteClient();

            agentClient.GetAll().ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        agentsHeaderTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(176, 100, 100));
                        agentsHeaderTextBlock.Text = "can't connect to remote server";
                        return;
                    }

                    agentsHeaderTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(106, 196, 234));
                    agentsHeaderTextBlock.Text = "select agents to assing to an Environment";

                    agentsDataGrid.ItemsSource = t.Result;
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }


        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            Opacity = 0.20;

            var registerAgent = new AddAgent();
            registerAgent.Owner = this;

            registerAgent.ShowDialog();

            Opacity = 1;
        }


        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
