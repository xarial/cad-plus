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

namespace Xarial.CadPlus.XBatch.Base.Converters
{
    public class LogOutputToTextConverter : IMultiValueConverter
    {
        private object m_Lock;

        public LogOutputToTextConverter() 
        {
            m_Lock = new object();
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                lock (m_Lock)
                {
                    var lines = values.First() as IEnumerable<string>;

                    var logText = new StringBuilder();

                    if (lines != null)
                    {
                        foreach (var line in lines)
                        {
                            logText.AppendLine(line?.ToString());
                        }
                    }

                    return logText.ToString();
                }
            }
            catch 
            {
                return Binding.DoNothing;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
