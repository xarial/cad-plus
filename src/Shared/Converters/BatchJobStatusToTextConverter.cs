using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Plus.Shared.Converters
{
    public class BatchJobStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BatchJobStatus_e)
            {
                switch ((BatchJobStatus_e)value)
                {
                    case BatchJobStatus_e.NotStarted:
                        return "Not Started";
                    case BatchJobStatus_e.Initializing:
                        return "Initializing";
                    case BatchJobStatus_e.InProgress:
                        return "In Progress";
                    case BatchJobStatus_e.Failed:
                        return "Failed";
                    case BatchJobStatus_e.Succeeded:
                        return "Succeeded";
                    case BatchJobStatus_e.CompletedWithWarning:
                        return "Completed With Warning";
                    case BatchJobStatus_e.Cancelled:
                        return "Cancelled";
                }
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
