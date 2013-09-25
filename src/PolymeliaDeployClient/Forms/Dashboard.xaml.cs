using System.Windows;

namespace PolymeliaDeployClient.Forms
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Controls;
    using System.Windows.Media;

    using PolymeliaDeploy.Controller;

    using Environment = PolymeliaDeploy.Data.Environment;

    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : UserControl
    {
        private readonly IActivityClient _client;

        public IEnumerable<Environment> Environments { get; set; }

        public Dashboard()
        {
            _client = new ActivityRemoteClient();

            InitializeComponent();
        }


        public void LoadHistory()
        {
            if (Environments == null || !Environments.Any())
                return;

            historyGrid.Children.Clear();
            historyGrid.RowDefinitions.Clear();

            historyGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
            historyGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(550) });

            AddEnvironmentColumns();
            AddLastestDeployHistoryForEnvironments();
        }


        private void AddLastestDeployHistoryForEnvironments()
        {
            var columnIndex = 0;

            var histoyDeployDataTemplate = FindResource("DeployHistoryDataTemplate") as DataTemplate;

            foreach (var environment in Environments)
            {
                var index = columnIndex;

                _client.GetDeployHistory(environment.ProjectId, environment.Id).ContinueWith(
                    history =>
                    {
                        if (!history.IsFaulted)
                        {
                            dashboardHeaderTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(106, 196, 234));
                            dashboardHeaderTextBlock.Text = "Show deployment history";

                            var historyListBox = new ItemsControl
                                                {
                                                     Height = 550,
                                                     Width = 260,
                                                     BorderBrush = null,
                                                     BorderThickness = new Thickness(0),
                                                     HorizontalAlignment = HorizontalAlignment.Left,
                                                     VerticalAlignment = VerticalAlignment.Top,
                                                     Margin = new Thickness(0)
                                                 };

                            Grid.SetRow(historyListBox, 1);
                            Grid.SetColumn(historyListBox, index);

                            historyListBox.ItemTemplate = histoyDeployDataTemplate;
                            historyListBox.ItemsSource = history.Result;

                            historyGrid.Children.Add(historyListBox);
                        }
                        else
                        {
                            dashboardHeaderTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(176, 100, 100));
                            dashboardHeaderTextBlock.Text = "Can't connect to remote server";
                        }

                    }, TaskScheduler.FromCurrentSynchronizationContext());

                columnIndex++;
            }
        }


        private void AddEnvironmentColumns()
        {
            historyGrid.ColumnDefinitions.Clear();

            var columnIndex = 0;

            foreach (var environment in Environments)
            {
                historyGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(260)});

                var column = new TextBlock { Text = environment.Name, FontSize = 18 };
                Grid.SetColumn(column, columnIndex);

                historyGrid.Children.Add(column);

                columnIndex++;
            }
        }


        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }
    }
}
