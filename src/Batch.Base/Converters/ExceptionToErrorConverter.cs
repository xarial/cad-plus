﻿//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Xarial.XToolkit.Reporting;

namespace Xarial.CadPlus.Batch.Base.Converters
{
    public class ExceptionToErrorConverter : IValueConverter
    {
        public static string Convert(Exception ex) 
        {
            if (ex is OperationCanceledException)
            {
                return "Operation cancelled";
            }
            else if (ex is TimeoutException)
            {
                return "Timeout";
            }
            else
            {
                return ex.ParseUserError(out _);
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Exception)
            {
                return Convert(value as Exception);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}