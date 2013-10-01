using System.Windows;

namespace PolymeliaDeployClient.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Windows.Controls;

    using PolymeliaDeploy.Controller;
    using PolymeliaDeploy.Data;

    using Environment = PolymeliaDeploy.Data.Environment;

    public partial class AddEnvironment : Window
    {

        public IEnumerable<Environment> Environments { get; set; }

        public Environment AddedEnvironment { get; private set; }

        public Environment CopyFromEnvironment { private get; set; }

        public Project CurrentProject { private get; set; }

        private ObservableCollection<Agent> Agents { get; set; }

        private readonly IEnvironmentClient _environmentClient;


        public string EnvironmentName
        {
            get { return environmentName.Text;  }
        }


        public AddEnvironment()
        {
            _environmentClient = new EnvironmentRemoteClient();

            InitializeComponent();

            Agents = new ObservableCollection<Agent>();

            agentsGrid.ItemsSource = Agents;

            environmentName.Focus();
        }


        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            copyEnvironmentComboBox.ItemsSource = Environments;
            copyEnvironmentComboBox.SelectedItem = CopyFromEnvironment;
        }


        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (Environments.Any(env => String.Equals(env.Name.Trim(), environmentName.Text.Trim(), StringComparison.CurrentCultureIgnoreCase)))
            {
                MessageBox.Show(string.Format("An environment with the name '{0}' already exists, please use another name", environmentName.Text), "Already exist");
                return;
            }

            if (MessageBox.Show("Are you sure you want to save?", "Save confirmation", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            var environment = new Environment
            {
                ProjectId = CurrentProject.Id,
                CreatedBy = Thread.CurrentPrincipal.Identity.Name,
                Description = descriptionTextBox.Text.Trim(),
                Name = environmentName.Text.Trim(),
                AssingedAgentIds = Agents.Select( a => a.Id).ToList()
            };

            try
            {
                AddedEnvironment = _environmentClient.AddEnvironment(environment);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception while trying to add environment, error: " + ex.Message, "Error white adding environment");
                return;
            }

            DialogResult = true;
        }


        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }


        private void environmentName_TextChanged(object sender, TextChangedEventArgs e)
        {
           okButton.IsEnabled = !string.IsNullOrWhiteSpace(environmentName.Text);
        }


        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            Opacity = 0.20;

            var selectAgents = new SelectAgents();
            selectAgents.Owner = this;

            var status = selectAgents.ShowDialog();

            if (status != null && status.Value)
            {
                var agents = selectAgents.SelectedAgents.ToList();

                foreach (var agent in agents.Where(agent => Agents.FirstOrDefault( a => a.Id == agent.Id) == null))
                    Agents.Add(agent);
            }

            Opacity = 1;
        }
    }
}
