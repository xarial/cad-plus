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
    public class JobItemStateToImageConverter : IValueConverter
    {
        private static readonly BitmapImage m_InitializingImage;
        private static readonly BitmapImage m_FailedImage;
        private static readonly BitmapImage m_InProgressImage;
        private static readonly BitmapImage m_SucceededImage;
        private static readonly BitmapImage m_WarningImage;

        static JobItemStateToImageConverter() 
        {
            m_InitializingImage = Resources.status_awaiting.ToBitmapImage();
            m_FailedImage = Resources.status_failed.ToBitmapImage();
            m_InProgressImage = Resources.status_in_progress.ToBitmapImage();
            m_SucceededImage = Resources.status_succeeded.ToBitmapImage();
            m_WarningImage = Resources.status_warning.ToBitmapImage();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is JobItemState_e)
            {
                switch ((JobItemState_e)value) 
                {
                    case JobItemState_e.Initializing:
                        return m_InitializingImage;
                    case JobItemState_e.Failed:
                        return m_FailedImage;
                    case JobItemState_e.InProgress:
                        return m_InProgressImage;
                    case JobItemState_e.Succeeded:
                        return m_SucceededImage;
                    case JobItemState_e.Warning:
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
