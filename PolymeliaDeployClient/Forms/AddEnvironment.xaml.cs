using System.Windows;

namespace PolymeliaDeployClient.Forms
{
    using System.Collections.Generic;
    using System.Windows.Controls;

    using PolymeliaDeploy.Data;

    public partial class AddEnvironment : Window
    {

        public IEnumerable<Environment> Environments { get; set; }


        public string EnvironmentName
        {
            get { return environmentName.Text;  }
        }


        public AddEnvironment()
        {
            InitializeComponent();

            environmentName.Focus();
        }


        protected override void OnContentRendered(System.EventArgs e)
        {
            base.OnContentRendered(e);

            copyEnvironmentComboBox.ItemsSource = Environments;
        }


        private void okButton_Click(object sender, RoutedEventArgs e)
        {
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
    }
}
