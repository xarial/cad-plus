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
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Batch.StandAlone.Properties;
using Xarial.CadPlus.Batch.StandAlone.ViewModels;
using Xarial.XToolkit.Wpf.Extensions;
using Xarial.CadPlus.Batch.Base.ViewModels;
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Batch.StandAlone.Converters
{
    public class JobStatusToImageConverter : IValueConverter
    {
        private static readonly BitmapImage m_InitiatingImage;
        private static readonly BitmapImage m_CancelledImage;
        private static readonly BitmapImage m_FailedImage;
        private static readonly BitmapImage m_InProgressImage;
        private static readonly BitmapImage m_SucceededImage;
        private static readonly BitmapImage m_WarningImage;

        static JobStatusToImageConverter() 
        {
            m_InitiatingImage = Resources.status_awaiting.ToBitmapImage();
            m_CancelledImage = Resources.status_cancelled.ToBitmapImage();
            m_FailedImage = Resources.status_failed.ToBitmapImage();
            m_InProgressImage = Resources.status_in_progress.ToBitmapImage();
            m_SucceededImage = Resources.status_succeeded.ToBitmapImage();
            m_WarningImage = Resources.status_warning.ToBitmapImage();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BatchJobStatus_e)
            {
                switch ((BatchJobStatus_e)value) 
                {
                    case BatchJobStatus_e.NotStarted:
                    case BatchJobStatus_e.Initializing:
                        return m_InitiatingImage;
                    case BatchJobStatus_e.Cancelled:
                        return m_CancelledImage;
                    case BatchJobStatus_e.Failed:
                        return m_FailedImage;
                    case BatchJobStatus_e.InProgress:
                        return m_InProgressImage;
                    case BatchJobStatus_e.Succeeded:
                        return m_SucceededImage;
                    case BatchJobStatus_e.CompletedWithWarning:
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
