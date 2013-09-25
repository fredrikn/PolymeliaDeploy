using System;

namespace PolymeliaDeployClient.Forms.Converters
{
    using System.Windows.Data;
    using System.Windows.Media;

    using PolymeliaDeploy.Data;

    public class DeploymentStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
                      System.Globalization.CultureInfo culture)
        {
            if (value is ActivityStatus)
            {
                Color color;

                var status = (ActivityStatus)value;

                switch (status)
                {
                    case ActivityStatus.Completed:
                        color = Color.FromRgb(200, 255, 200);
                        break;
                    case ActivityStatus.Failed:
                        color = Color.FromRgb(255, 180, 180);
                        break;
                    default:
                        color = Color.FromRgb(200, 200, 200);
                        break;
                }

                return new SolidColorBrush(color);
            }

            return new SolidColorBrush(Color.FromRgb(255,255,255));
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                      System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
