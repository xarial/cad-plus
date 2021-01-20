﻿//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Xarial.CadPlus.Plus.Shared.Styles
{
    public static class ApplicationExtension
    {
        public static void UsingMetroStyles(this Application app)
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
