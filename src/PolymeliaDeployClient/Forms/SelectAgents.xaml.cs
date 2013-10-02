using System.Windows;

namespace PolymeliaDeployClient.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Media;

    using PolymeliaDeploy.Agent;
    using PolymeliaDeploy.Controller;
    using PolymeliaDeploy.Data;

    /// <summary>
    /// Interaction logic for SelectAgents.xaml
    /// </summary>
    public partial class SelectAgents : Window
    {
        public IEnumerable<Agent> SelectedAgents { get; private set; }

        public ObservableCollection<Agent> Agents { get; private set; }

        public IEnumerable<Agent> ExcludAgents { private get; set; }

        public IEnumerable<Agent> IncludeAgents { private get; set; }

        public SelectAgents()
        {
            Agents = new ObservableCollection<Agent>();
            ExcludAgents = new List<Agent>();
            IncludeAgents = new List<Agent>();

            InitializeComponent();
        }


        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            var agentClient = new AgentRemoteClient();

            agentClient.GetAllUnassigned().ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        agentsHeaderTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(176, 100, 100));
                        agentsHeaderTextBlock.Text = "can't connect to remote server";
                        return;
                    }

                    agentsHeaderTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(106, 196, 234));
                    agentsHeaderTextBlock.Text = "select agents to assign to an Environment";

                    foreach (var agent in t.Result.Where(agent => ExcludAgents.All(a => a.Id != agent.Id)))
                        Agents.Add(agent);

                    foreach (var includeAgent in IncludeAgents.Where(includeAgent => Agents.All(a => a.Id != includeAgent.Id)))
                        this.Agents.Add(includeAgent);

                }, TaskScheduler.FromCurrentSynchronizationContext());
        }


        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            Opacity = 0.20;

            var registerAgent = new AddAgent();
            registerAgent.Owner = this;

            if (registerAgent.ShowDialog() == true)
                Agents.Add(registerAgent.AddedAgent);

            Opacity = 1;
        }


        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }


        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            var agents = new List<Agent>();
            
            foreach(var selectedAgent in agentsDataGrid.SelectedItems)
                agents.Add(selectedAgent as Agent);

            SelectedAgents = agents;

            DialogResult = true;
        }


        private void agentsDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            okButton.IsEnabled = (e.AddedItems != null && e.AddedItems.Count > 0);
        }
    }
}