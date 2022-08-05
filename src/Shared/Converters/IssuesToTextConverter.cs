//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Plus.Shared.Converters
{
    public class IssuesToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IReadOnlyList<IJobItemIssue>)
            {
                var issues = new StringBuilder();

                foreach (var issue in (IReadOnlyList<IJobItemIssue>)value) 
                {
                    if (issues.Length != 0) 
                    {
                        issues.AppendLine();
                    }

                    issues.Append(issue.Content?.ToString());
                }

                return issues.ToString();
            }
            else 
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
