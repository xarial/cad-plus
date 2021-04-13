//*********************************************************************
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
using Xarial.CadPlus.Batch.Base.ViewModels;
using Xarial.CadPlus.Common.Utils;

namespace Xarial.CadPlus.Batch.Base.Converters
{
    public class ArgumentsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var args = value as string;

            if (!string.IsNullOrEmpty(args))
            {
                var argsArr = CommandLineHelper.ParseCommandLine(args);

                var argsVm = new ArgumentVM[argsArr.Length];

                for (int i = 0; i < argsArr.Length; i++) 
                {
                    argsVm[i] = new ArgumentVM()
                    {
                        Index = i + 1,
                        Value = argsArr[i]
                    };
                }

                return argsVm;
            }
            else 
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
