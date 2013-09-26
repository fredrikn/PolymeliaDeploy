using System.Windows;

namespace PolymeliaDeployClient.Forms
{
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for AddAgent.xaml
    /// </summary>
    public partial class AddAgent : Window
    {
        public AddAgent()
        {
            InitializeComponent();

            roleName.Focus();
        }


        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }


        private void saveButton_Click(object sender, RoutedEventArgs e)
        {

        }


        private void RoleName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            saveButton.IsEnabled = IsValid();
        }


        private void ServerName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            saveButton.IsEnabled = IsValid();
        }

        private void AgentToken_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            saveButton.IsEnabled = IsValid();
        }


        private bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(roleName.Text) &&
                   !string.IsNullOrWhiteSpace(serverName.Text) &&
                   !string.IsNullOrWhiteSpace(agentToken.Text);
        }
    }
}
