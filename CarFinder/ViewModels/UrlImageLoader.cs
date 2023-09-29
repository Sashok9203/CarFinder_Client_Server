using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace CarFinder.ViewModels
{
    public class UrlImageLoader : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && !string.IsNullOrEmpty(str))
            {
                BitmapImage bi = new();
                bi.BeginInit();
                bi.UriSource = new(str, UriKind.Absolute);
                bi.EndInit();
                return bi;
            }
            return Resource.no_vehicle;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture){return false;}
    }
}
