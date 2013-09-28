using System.Windows;
using System.Windows.Controls;

namespace PolymeliaDeployClient
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading;

    using PolymeliaDeploy.Controller;
    using PolymeliaDeploy.Data;

    using PolymeliaDeployClient.Forms;

    using Environment = PolymeliaDeploy.Data.Environment;

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly IEnvironmentClient _environmentClient;

        private readonly IAgentRemoteClient _agentRemoteClient;

        private readonly ObservableCollection<Environment> _environments = new ObservableCollection<Environment>();

        private Environment _selectedEnvironment;

        private Project _currentProject;

        public event PropertyChangedEventHandler PropertyChanged;
  


        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            _environmentClient = new EnvironmentRemoteClient();
            _agentRemoteClient = new AgentRemoteClient();

            environmentTabs.ItemsSource = _environments;
        }


        public Project CurrentProject
        {
            get
            {
                return _currentProject;
            }
            set
            {
                _currentProject = value;

                addButton.IsEnabled = value != null;

                if (value != null)
                    Title = value.Name + " - Polymelia Deploy Client v 0.1";
            }
        }


        public Environment SelectedEnvironment
        {
            get
            {
                return _selectedEnvironment;
            }
            set
            {
                _selectedEnvironment = value;

                variableButton.IsEnabled = value != null;
                deployButton.IsEnabled = value != null;
                saveButton.IsEnabled = value != null;

                OnPropertyChanged("SelectedEnvironment");
            }
        }


        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            OpenAndSelectProjectWindow();

            messageTextBlock.Text = "Ready";
        }


        private void OpenAndSelectProjectWindow()
        {
            AlertOnUnsavedChanges();

            blockBackgroundGrid.Visibility = Visibility.Visible;

            var selectProject = new SelectProject();
            selectProject.Owner = this;

            if (selectProject.ShowDialog() == true)
            {
                CurrentProject = selectProject.ProjectSelected;

                ReloadProjectEnvironments();

                messageTextBlock.Text = string.Format("Project '{0}' is opened", CurrentProject.Name);
            }

            blockBackgroundGrid.Visibility = Visibility.Hidden;

        }


        private async void ReloadProjectEnvironments()
        {
            _environments.Clear();

            var environments = await _environmentClient.GetProjectsEnvironments(CurrentProject.Id);

            if (environments != null && environments.Any())
            {
                foreach (var environment in environments)
                    _environments.Add(environment);

                SelectedEnvironment = _environments[0];
            }
            else
                SelectedEnvironment = null;

            addButton.IsEnabled = true;
        }


        private void deployButton_Click(object sender, RoutedEventArgs e)
        {
            var deploy = new Deploy();
            deploy.SelectedEnvironment = SelectedEnvironment;
            deploy.Owner = this;

            deploy.ShowDialog();
        }


        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Do you want to save?", "Save", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            Save();

            messageTextBlock.Text = "Item(s) Saved";
        }


        private void Save()
        {
            foreach (var environment in _environments)
            {
                if (environment.Id == 0)
                {
                    var addedEnv = _environmentClient.AddEnvironment(environment);
                    environment.Id = addedEnv.Id;
                }
                else
                {
                    if (environment.HasChanges)
                        _environmentClient.UpdateEnvironment(environment);
                }

                environment.HasChanges = false;
            }
        }


        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            OpenAndSelectProjectWindow();
        }


        private bool SomethingToSave()
        {
            return _environments.Any(e => e.Id == 0 || e.HasChanges);
        }


        private void AddNewEnvironment()
        {
            var win = new AddEnvironment();
            win.Owner = this;

            win.Environments = _environments;

            if (win.ShowDialog() == true)
            {
                var environment = new Environment
                                      {
                                          ProjectId = CurrentProject.Id,
                                          CreatedBy = Thread.CurrentPrincipal.Identity.Name,
                                          Description = "",
                                          Name = win.EnvironmentName,
                                      };

                environment = _environmentClient.AddEnvironment(environment);

                _environments.Add(environment);

                SelectedEnvironment = environment;

                messageTextBlock.Text = string.Format("Environment '{0}' is created", SelectedEnvironment.Name);
            }
        }


        private void newButton_Click(object sender, RoutedEventArgs e)
        {
            AlertOnUnsavedChanges();

            blockBackgroundGrid.Visibility = Visibility.Visible;

            var newProjectWindow = new NewProject();
            newProjectWindow.Owner = this;

            if (newProjectWindow.ShowDialog() == true)
            {
                CurrentProject = newProjectWindow.ProjectCreated;

                ReloadProjectEnvironments();
            }

            messageTextBlock.Text = "Ready";

            blockBackgroundGrid.Visibility = Visibility.Hidden;
        }


        protected void OnPropertyChanged(string name)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(name));
        }


        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            blockBackgroundGrid.Visibility = Visibility.Visible;

            AddNewEnvironment();

            blockBackgroundGrid.Visibility = Visibility.Hidden;
        }


        private void RemoveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            if (mi == null)
                return;

            var cm = mi.CommandParameter as ContextMenu;
            if (cm == null)
                return;

            var tabItem = cm.PlacementTarget as TabItem;
            if (tabItem == null)
                return;

            var environment = tabItem.Header as Environment;

            if (environment != null)
            {
                if (MessageBox.Show(
                    string.Format("Do you really want to remove environment '{0}'?", environment.Name),
                    string.Format("Remove environment '{0}'", environment.Name),
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _environmentClient.DeleteEnvironment(environment);
                    
                    messageTextBlock.Text = string.Format("Environment '{0}' is deleted", SelectedEnvironment.Name);

                    _environments.Remove(environment);

                    addButton.IsEnabled = _environments.Any();

                    SelectedEnvironment = _environments.FirstOrDefault();
                }
            }
        }


        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            AlertOnUnsavedChanges();
        }


        private void AlertOnUnsavedChanges()
        {
            if (SomethingToSave())
            {
                if (MessageBox.Show("Do you want to save the unsaved changes?", "Unsaved changes", MessageBoxButton.YesNo)
                    == MessageBoxResult.Yes)
                {
                    Save();
                }
            }
        }


        private void variableButton_Click(object sender, RoutedEventArgs e)
        {
            var variables = new Variables();
            
            variables.Owner = this;
            variables.EnvironmentId = SelectedEnvironment.Id;

            blockBackgroundGrid.Visibility = Visibility.Visible;
            
            variables.ShowDialog();

            blockBackgroundGrid.Visibility = Visibility.Hidden;
        }


        private void viewSettingsButton_Click(object sender, RoutedEventArgs e)
        {
        }


        private void mainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tabItem = mainTabs.SelectedItem as TabItem;

            if (tabItem != null && tabItem.Tag.ToString() == "deployments")
            {
                tabItem.Content = null;

                var dashboard = new Deployments();

                dashboard.Environments = _environments;
                dashboard.LoadHistory();

                tabItem.Content = dashboard;
            }
            else if (tabItem != null && tabItem.Tag.ToString() == "agents")
            {
                tabItem.Content = null;

                var listAgents = new ListAgents(_agentRemoteClient);
                listAgents.LoadAgents();

                tabItem.Content = listAgents;
            }
        }
    }
}