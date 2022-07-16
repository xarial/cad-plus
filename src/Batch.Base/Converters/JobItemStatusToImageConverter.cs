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
using Xarial.CadPlus.Batch.Base.Properties;
using Xarial.CadPlus.Common.Services;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Batch.Base.Converters
{
    public class JobItemStatusToImageConverter : IValueConverter
    {
        private static readonly BitmapImage m_AwaitingProcessingImage;
        private static readonly BitmapImage m_FailedImage;
        private static readonly BitmapImage m_InProgressImage;
        private static readonly BitmapImage m_SucceededImage;
        private static readonly BitmapImage m_WarningImage;

        static JobItemStatusToImageConverter() 
        {
            m_AwaitingProcessingImage = Resources.status_awaiting.ToBitmapImage();
            m_FailedImage = Resources.status_failed.ToBitmapImage();
            m_InProgressImage = Resources.status_in_progress.ToBitmapImage();
            m_SucceededImage = Resources.status_succeeded.ToBitmapImage();
            m_WarningImage = Resources.status_warning.ToBitmapImage();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is JobItemStatus_e)
            {
                switch ((JobItemStatus_e)value) 
                {
                    case JobItemStatus_e.AwaitingProcessing:
                        return m_AwaitingProcessingImage;
                    case JobItemStatus_e.Failed:
                        return m_FailedImage;
                    case JobItemStatus_e.InProgress:
                        return m_InProgressImage;
                    case JobItemStatus_e.Succeeded:
                        return m_SucceededImage;
                    case JobItemStatus_e.Warning:
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
