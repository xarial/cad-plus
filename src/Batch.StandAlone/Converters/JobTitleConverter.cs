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

namespace Xarial.CadPlus.Batch.StandAlone.Converters
{
    public class JobTitleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is string && values[1] is int && values[2] is int && values[3] is int)
            {
                var jobName = (string)values[0];
                var procFilesCount = (int)values[1];
                var failedFilesCount = (int)values[2];
                var totalFilesCount = (int)values[3];

                double prg = 0;

                if (totalFilesCount != 0)
                {
                    prg = ((procFilesCount + failedFilesCount) / (double)totalFilesCount) * 100;
                }

                return $"{jobName} - {prg:F2}% - {procFilesCount + failedFilesCount}/{totalFilesCount} file(s)";
            }
            else 
            {
                return "";
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
