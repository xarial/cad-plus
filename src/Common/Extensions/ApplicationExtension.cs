using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Xarial.CadPlus.Common.Extensions
{
    public static class ApplicationExtension
    {
        public static void WithMetroStyles(this Application app) 
        {
            app.Resources.MergedDictionaries.Add(
             new ResourceDictionary()
             {
                 Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Controls.xaml")
             });

            app.Resources.MergedDictionaries.Add(
                new ResourceDictionary()
                {
                    Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Fonts.xaml")
                });

            app.Resources.MergedDictionaries.Add(
                new ResourceDictionary()
                {
                    Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Themes/Light.Blue.xaml")
                });

            app.Resources.MergedDictionaries.Add(
                new ResourceDictionary()
                {
                    Source = new Uri("pack://application:,,,/Fluent;Component/Themes/Generic.xaml")
                });
        }
    }
}
