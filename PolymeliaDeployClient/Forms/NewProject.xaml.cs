using System.Windows;

namespace PolymeliaDeployClient.Forms
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Controls;

    using PolymeliaDeploy.Controller;
    using PolymeliaDeploy.Data;

    public partial class NewProject : Window
    {
        IProjectClient _client;

        public Project ProjectCreated { get; private set; }


        public IEnumerable<Project> PreviousProjects { get; set; }


        public string ProejctName
        {
            get { return projectName.Text;  }
        }

        public NewProject()
        {
            InitializeComponent();

            _client = new ProjectRemoteClient();

            projectName.Focus();
        }


        protected override void OnContentRendered(System.EventArgs e)
        {
            base.OnContentRendered(e);

            if (PreviousProjects == null)
            {
                GetAllProjects();
            }
            else
                copyProjectComboBox.ItemsSource = PreviousProjects;
        }


        private async Task GetAllProjects()
        {
            var result = await _client.GetAllProjects();
            
            copyProjectComboBox.ItemsSource = result;
        }


        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            int? copyProjectId = null;

            var value = copyProjectComboBox.SelectedItem;

            if (value != null)
            {
                var selectedProctToCopyFrom = value as Project;
                if (selectedProctToCopyFrom != null)
                    copyProjectId = selectedProctToCopyFrom.Id;
            }

            var project = new Project
            {
                CreatedBy = Thread.CurrentPrincipal.Identity.Name,
                Name = projectName.Text,
                Description = descriptionTextBox.Text
            };

            var createdProject = _client.CreateProject(project, copyProjectId);

            ProjectCreated = createdProject;

            DialogResult = true;
        }


        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }


        private void projectName_TextChanged(object sender, TextChangedEventArgs e)
        {
           okButton.IsEnabled = !string.IsNullOrWhiteSpace(projectName.Text);
        }
    }
}
