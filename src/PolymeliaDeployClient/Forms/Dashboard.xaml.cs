using System.Windows;

namespace PolymeliaDeployClient.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Controls;
    using System.Windows.Media;

    using PolymeliaDeploy.Controller;
    using PolymeliaDeploy.Data;

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

            AddEnvironmentColumns();
            AddLastestDeployHistoryForEnvironments();


            try
            {
                dashboardHeaderTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(106, 196, 234));
                dashboardHeaderTextBlock.Text = "Show deployment history";
            }
            catch (Exception)
            {
                dashboardHeaderTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(176, 100, 100));
                dashboardHeaderTextBlock.Text = "Can't connect to remote server";
            }
        }


        private void AddLastestDeployHistoryForEnvironments()
        {
            var columnIndex = 0;

            foreach (var environment in Environments)
            {
                var index = columnIndex;

                _client.GetDeployHistory(environment.ProjectId, environment.Id).ContinueWith(
                    history =>
                    {
                        if (!history.IsFaulted)
                        {
                            var rowIndex = 1;
                            
                            foreach (var mainActivity in history.Result)
                            {
                                historyGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(100) });

                                historyGrid.Children.Add(
                                    CreateDeployVersionBox(
                                        mainActivity,
                                        gridRow: rowIndex,
                                        gridColumn: index,
                                        color: ConvertStatusToColor(mainActivity)));

                                rowIndex++;
                            }
                        }
                    }, TaskScheduler.FromCurrentSynchronizationContext());

                columnIndex++;
            }
        }

        private static Color ConvertStatusToColor(MainActivity mainActivity)
        {
            Color color;

            if (mainActivity.Status == ActivityStatus.Completed)
                color = Color.FromRgb(200, 255, 200);
            else if (mainActivity.Status == ActivityStatus.Failed)
                color = Color.FromRgb(255, 180, 180);
            else
                color = Color.FromRgb(200, 200, 200);

            return color;
        }

        private static UIElement CreateDeployVersionBox(
                                                        MainActivity mainActivity,
                                                        Color color,
                                                        int gridColumn = 0,
                                                        int gridRow = 0)
        {
            var box = new StackPanel
                      {
                          Background = new SolidColorBrush(color),
                          Width = 250,
                          Height = 80,
                          Margin = new Thickness(0, 10, 50, 10),
                          HorizontalAlignment = HorizontalAlignment.Left,
                          VerticalAlignment = VerticalAlignment.Top
                      };

            box.Children.Add(new TextBlock
                             {
                                 FontSize = 14,
                                 Text = "VERSION: " + mainActivity.Version,
                                 HorizontalAlignment = HorizontalAlignment.Left,
                                 Margin = new Thickness(8, 8, 8, 4)
                             });

            box.Children.Add(new TextBlock
                            {
                                FontSize = 14,
                                Text = mainActivity.Created.ToString("ddd MMM M yyyy hh:mm tt"),
                                HorizontalAlignment = HorizontalAlignment.Left,
                                Margin = new Thickness(8, 4, 8, 4)
                            });

            box.Children.Add(new TextBlock
                            {
                                FontSize = 12,
                                Text = "DEPLOYED BY: " + mainActivity.CreatedBy,
                                HorizontalAlignment = HorizontalAlignment.Left,
                                Margin = new Thickness(8, 4 ,8, 8)
                            });

            Grid.SetColumn(box, gridColumn);
            Grid.SetRow(box, gridRow);

            return box;
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
