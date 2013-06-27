using System.Windows;

namespace PolymeliaDeployClient.Forms
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using System.Windows.Media;

    using PolymeliaDeploy.Controller;
    using PolymeliaDeploy.Data;


    public partial class Variables : Window
    {
        IVariableClient _client;

        public int EnvironmentId { private get; set; }

        ObservableCollection<Variable> _variables = new ObservableCollection<Variable>();


        public Variables()
        {
            InitializeComponent();
        }


        protected override void OnContentRendered(System.EventArgs e)
        {
            base.OnContentRendered(e);

            _client = new VariableRemoteClient();

            Task<IEnumerable<Variable>>.Run(() => _client.GetAllVariables(EnvironmentId)).ContinueWith(
                result =>
                {
                    _variables = new ObservableCollection<Variable>(result.Result);
                    varaibleHeaderTextBlock.Text = "Add or update variables";
                    varaibleHeaderTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(106, 196, 234));

                    varaiblesGrid.ItemsSource = _variables;
                    varaiblesGrid.BeginEdit();
                },
                TaskScheduler.FromCurrentSynchronizationContext());
        }


        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }


        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            var variables = new Collection<Variable>();

            if (varaiblesGrid.Items != null && varaiblesGrid.Items.Count > 0)
            {
                foreach (var item in varaiblesGrid.Items)
                {
                    var variable = item as Variable;

                    if (variable != null)
                    {
                        variable.EnvironmentId = EnvironmentId;
                        variables.Add(variable);
                    }
                }
            }

            _client.AddVariables(variables, EnvironmentId);
            DialogResult = true;
        }


        private void varaiblesGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            removeButton.IsEnabled = true;
        }

        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_variables.Count == 0)
                return;

            _variables.Remove(varaiblesGrid.SelectedItem as Variable);

            if (_variables.Count == 0)
                removeButton.IsEnabled = false;
        }
    }
}
