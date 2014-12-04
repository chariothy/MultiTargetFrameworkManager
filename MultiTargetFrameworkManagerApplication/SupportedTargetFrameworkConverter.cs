using Nu.Vs.Extension;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MultiTargetFrameworkManagerApplication
{
    public class SupportedTargetFrameworkConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var suppertedTargetFrameworks = value as ObservableCollection<FrameworkDefinition>;
            if (suppertedTargetFrameworks == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder("Target: ");
            foreach (var item in suppertedTargetFrameworks)
            {
                if (item.Selected)
                {
                    sb.Append(item.NugetAbbreviation);
                    sb.Append("; "); 
                }
            }
            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
