using System;

namespace PolymeliaDeployClient.Forms.Converters
{
    using System.Windows.Data;
    using System.Windows.Media;

    using PolymeliaDeploy.Agent;

    public class AgentStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
                      System.Globalization.CultureInfo culture)
        {
            if (value is AgentStatus)
            {
                Color color;

                var status = (AgentStatus)value;

                switch (status)
                {
                    case AgentStatus.InProgress:
                        color = Color.FromRgb(200, 255, 200);
                        break;
                    case AgentStatus.NotActive:
                        color = Color.FromRgb(255, 180, 180);
                        break;
                    case AgentStatus.Ready:
                        color = Color.FromRgb(106, 196, 234);
                        break;
                    default:
                        color = Color.FromRgb(200, 200, 200);
                        break;
                }

                return new SolidColorBrush(color);
            }

            return new SolidColorBrush(Color.FromRgb(255, 255, 255));
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                      System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
