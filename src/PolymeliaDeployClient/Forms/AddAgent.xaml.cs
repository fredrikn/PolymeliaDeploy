using System.Windows;

namespace PolymeliaDeployClient.Forms
{
    using System;
    using System.Threading;
    using System.Windows.Controls;

    using PolymeliaDeploy.Controller;
    using PolymeliaDeploy.Data;

    public partial class AddAgent : Window
    {
        private IAgentRemoteClient _agentRemoteClient;

        public Agent AddedAgent { get; private set; }

        public AddAgent()
        {
            InitializeComponent();

            _agentRemoteClient = new AgentRemoteClient();

            roleName.Focus();
        }


        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }


        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(
                "Are you sure you want to add the agent?",
                "Add agent confirmation",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    var addedAgent = new Agent
                                         {
                                             Confirmed = DateTime.Now,
                                             ConfirmedBy = Thread.CurrentPrincipal.Identity.Name,
                                             Role = roleName.Text.Trim(),
                                             ServerName = serverName.Text.Trim(),
                                             IpAddress = string.Empty,
                                             IsActive =
                                                 IsActiveCheckBox.IsChecked.HasValue
                                                 && IsActiveCheckBox.IsChecked.Value
                                         };

                    AddedAgent = _agentRemoteClient.Add(addedAgent);

                    DialogResult = true;
                }
                catch (Exception)
                {
                    MessageBox.Show("Can't add agent, please try again.!");
                }
            }
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