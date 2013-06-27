using System.Windows;
using System.Windows.Controls;

namespace PolymeliaDeployClient.Forms
{
    using System;
    using System.Collections.ObjectModel;
    using System.Windows.Media;

    using PolymeliaDeploy.Controller;
    using PolymeliaDeploy.Data;

    /// <summary>
    /// Interaction logic for SelectProject.xaml
    /// </summary>
    public partial class SelectProject : Window
    {
        IProjectClient _client;

        public Project ProjectSelected { get; private set; }

        ObservableCollection<Project> _projects = new ObservableCollection<Project>(); 

        public SelectProject()
        {
            InitializeComponent();
        }


        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            LoadProjects();
        }


        private async void LoadProjects()
        {
            _client = new ProjectRemoteClient();

            try
            {
                var result = await _client.GetAllProjects();

                _projects = new ObservableCollection<Project>(result);
                projectsListBox.ItemsSource = _projects;

                selectProjectTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(106, 196, 234));
                selectProjectTextBlock.Text = "Select a project";
            }
            catch (Exception)
            {
                selectProjectTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(176, 100, 100));
                selectProjectTextBlock.Text = "Can't connect to remote server";
            }
        }


        private void projectsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems == null || e.AddedItems.Count == 0)
                return;

            ProjectSelected = e.AddedItems[0] as Project;
            
            okButton.IsEnabled = ProjectSelected != null;
            removeProjectButton.IsEnabled = ProjectSelected != null;
        }


        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }


        private void createNew_Click(object sender, RoutedEventArgs e)
        {
            var newProject = new NewProject();

            newProject.Owner = this;
            newProject.PreviousProjects = _projects;

            if (newProject.ShowDialog() == true)
            {
                ProjectSelected = newProject.ProjectCreated;
                
                _projects.Add(ProjectSelected);

                projectsListBox.SelectedItem = ProjectSelected;
            }
        }

        private void projectsListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (projectsListBox.SelectedItem == null)
                return;

            ProjectSelected = projectsListBox.SelectedItem as Project;
            DialogResult = true;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }


        private void removeProjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(
                            string.Format("Are you sure you want to remove the project '{0}'\nand all its environments?", ProjectSelected.Name),
                            string.Format("Remove project '{0}'", ProjectSelected.Name),
                            MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _client.DeleteProject(ProjectSelected);

                _projects.Remove(ProjectSelected);

                ProjectSelected = null;

                okButton.IsEnabled = ProjectSelected != null;
                removeProjectButton.IsEnabled = ProjectSelected != null;
            }
        }
    }
}
