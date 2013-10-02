using System.Windows;
using System.Windows.Controls;

namespace PolymeliaDeployClient.Forms
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Media;

    using PolymeliaDeploy.Controller;
    using PolymeliaDeploy.Data;

    /// <summary>
    /// Interaction logic for EditEnvironment.xaml
    /// </summary>
    public partial class EditEnvironment : Window , INotifyPropertyChanged
    {
        private Environment _selectedEnvironment;

        private IAgentRemoteClient _agentClient;

        public ObservableCollection<Agent> Agents { get; private set; }

        private IList<Agent> _removedAgents = new List<Agent>();

        public Environment SelectedEnvironment
        {
            get
            {
                return _selectedEnvironment;
            }
            set
            {
                _selectedEnvironment = value;

                LoadAgentsForSelectedEnvironment();

                OnPropertyChanged("SelectedEnvironment");
            }
        }


        public EditEnvironment()
        {
            _agentClient = new AgentRemoteClient();

            Agents = new ObservableCollection<Agent>();

            InitializeComponent();
        }


        private void environmentName_TextChanged(object sender, TextChangedEventArgs e)
        {
            okButton.IsEnabled = !string.IsNullOrEmpty(environmentName.Text);
        }


        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }


        private void LoadAgentsForSelectedEnvironment()
        {
            _agentClient.GetAllAgentsForEnvironment(this._selectedEnvironment.Id).ContinueWith(
                a =>
                {
                    if (!a.IsFaulted)
                    {
                        loadingAgentsBlock.Visibility = Visibility.Collapsed;

                        foreach (var agent in a.Result)
                            Agents.Add(agent);
                    }
                    else
                    {
                        loadingAgentsBlock.Foreground = new SolidColorBrush(Color.FromRgb(176, 100, 100));
                        loadingAgentsBlock.Text = "Can't connect to remote server";
                    }
                },
                TaskScheduler.FromCurrentSynchronizationContext());
        }


        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            Opacity = 0.20;

            var selectAgents = new SelectAgents();
            selectAgents.Owner = this;
            selectAgents.ExcludAgents = Agents;
            selectAgents.IncludeAgents = _removedAgents;

            if (selectAgents.ShowDialog() == true)
            {
                var agents = selectAgents.SelectedAgents.ToList();

                foreach (var agent in agents.Where(agent => Agents.FirstOrDefault(a => a.Id == agent.Id) == null))
                {
                    Agents.Add(agent);

                    var agentRemove = _removedAgents.SingleOrDefault(a => a.Id == agent.Id);
                    if (agentRemove != null)
                        _removedAgents.Remove(agentRemove);
                }
            }

            Opacity = 1;
        }


        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var selectedAgent in agentsGrid.SelectedItems.OfType<Agent>().ToList())
            {
                Agents.Remove(selectedAgent);
                
                if (_removedAgents.All(a => a.Id != selectedAgent.Id))
                    _removedAgents.Add(selectedAgent);
            }
        }

        private void agentsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            removeButton.IsEnabled = agentsGrid.SelectedItem != null;
        }
    }
}