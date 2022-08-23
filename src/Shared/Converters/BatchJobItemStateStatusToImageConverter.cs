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
using System.Windows.Media.Imaging;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Shared.Properties;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Plus.Shared.Converters
{
    public class BatchJobItemStateStatusToImageConverter : IValueConverter
    {
        private static readonly BitmapImage m_QueuedImage;
        private static readonly BitmapImage m_FailedImage;
        private static readonly BitmapImage m_InProgressImage;
        private static readonly BitmapImage m_SucceededImage;
        private static readonly BitmapImage m_WarningImage;

        static BatchJobItemStateStatusToImageConverter() 
        {
            m_QueuedImage = Resources.status_awaiting.ToBitmapImage(true);
            m_FailedImage = Resources.status_failed.ToBitmapImage(true);
            m_InProgressImage = Resources.status_in_progress.ToBitmapImage(true);
            m_SucceededImage = Resources.status_succeeded.ToBitmapImage(true);
            m_WarningImage = Resources.status_warning.ToBitmapImage(true);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BatchJobItemStateStatus_e)
            {
                switch ((BatchJobItemStateStatus_e)value) 
                {
                    case BatchJobItemStateStatus_e.Queued:
                        return m_QueuedImage;
                    case BatchJobItemStateStatus_e.Failed:
                        return m_FailedImage;
                    case BatchJobItemStateStatus_e.InProgress:
                        return m_InProgressImage;
                    case BatchJobItemStateStatus_e.Succeeded:
                        return m_SucceededImage;
                    case BatchJobItemStateStatus_e.Warning:
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
