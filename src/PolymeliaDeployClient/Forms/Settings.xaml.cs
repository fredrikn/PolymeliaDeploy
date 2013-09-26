using System.Windows;
using System.Windows.Controls;

namespace PolymeliaDeployClient.Forms
{
    using System;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using System.Windows.Media;

    using PolymeliaDeploy.Controller;
    using PolymeliaDeploy.Data;

    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl
    {
        private readonly IAgentRemoteClient _agentRemoteClient;

        public Settings(IAgentRemoteClient agentRemoteClient)
        {
            if (agentRemoteClient == null) throw new ArgumentNullException("agentRemoteClient");

            _agentRemoteClient = agentRemoteClient;

            Agents = new ObservableCollection<Agent>();

            InitializeComponent();
        }


        public void LoadAgents()
        {
            _agentRemoteClient.GetAll().ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        settingsHeaderTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(176, 100, 100));
                        settingsHeaderTextBlock.Text = "Can't connect to remote server";
                        return;
                    }

                    settingsHeaderTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(106, 196, 234));
                    settingsHeaderTextBlock.Text = "Show settings";

                    Agents.Clear();

                    foreach (var agent in t.Result)
                        Agents.Add(agent);                        

                }, TaskScheduler.FromCurrentSynchronizationContext());
        }


        public ObservableCollection<Agent> Agents { get; set; } 


        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }


        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            var registerAgent = new AddAgent();
            registerAgent.Owner = Window.GetWindow(this);

            var blockBackgroundGrid = (Grid)Window.GetWindow(this).FindName("blockBackgroundGrid");

            blockBackgroundGrid.Visibility = Visibility.Visible;

            registerAgent.ShowDialog();

            blockBackgroundGrid.Visibility = Visibility.Hidden;

        }

        private void removeButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
