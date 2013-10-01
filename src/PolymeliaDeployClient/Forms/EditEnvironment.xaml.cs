using System.Windows;
using System.Windows.Controls;

namespace PolymeliaDeployClient.Forms
{
    using System.ComponentModel;

    using PolymeliaDeploy.Data;

    /// <summary>
    /// Interaction logic for EditEnvironment.xaml
    /// </summary>
    public partial class EditEnvironment : Window , INotifyPropertyChanged
    {
        private Environment selectedEnvironment;

        public Environment SelectedEnvironment
        {
            get
            {
                return this.selectedEnvironment;
            }
            set
            {
                this.selectedEnvironment = value;
                OnPropertyChanged("SelectedEnvironment");
            }
        }


        public EditEnvironment()
        {
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
    }
}
