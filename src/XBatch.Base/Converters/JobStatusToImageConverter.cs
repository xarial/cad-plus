using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.XBatch.Base.Properties;
using Xarial.CadPlus.XBatch.Base.ViewModels;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.XBatch.Base.Converters
{
    public class JobStatusToImageConverter : IValueConverter
    {
        private static readonly BitmapImage m_CancelledImage;
        private static readonly BitmapImage m_FailedImage;
        private static readonly BitmapImage m_InProgressImage;
        private static readonly BitmapImage m_SucceededImage;
        private static readonly BitmapImage m_WarningImage;

        static JobStatusToImageConverter() 
        {
            m_CancelledImage = Resources.status_cancelled.ToBitmapImage();
            m_FailedImage = Resources.status_failed.ToBitmapImage();
            m_InProgressImage = Resources.status_in_progress.ToBitmapImage();
            m_SucceededImage = Resources.status_succeeded.ToBitmapImage();
            m_WarningImage = Resources.status_warning.ToBitmapImage();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is JobState_e)
            {
                switch ((JobState_e)value) 
                {
                    case JobState_e.Cancelled:
                        return m_CancelledImage;
                    case JobState_e.Failed:
                        return m_FailedImage;
                    case JobState_e.InProgress:
                        return m_InProgressImage;
                    case JobState_e.Succeeded:
                        return m_SucceededImage;
                    case JobState_e.CompletedWithWarning:
                        return m_WarningImage;
                    default:
                        return null;
                }
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
