//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
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
            var jobName = values[0] as string;

            if (values[1] is int && values[2] is int && values[3] is int)
            {
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
                return jobName;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
