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
using System.Windows.Media.Imaging;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Shared.Properties;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Plus.Shared.Converters
{
    public class BatchJobItemIssueTypeToImageConverter : IValueConverter
    {
        private static readonly BitmapImage m_InfoImage;
        private static readonly BitmapImage m_WarningImage;
        private static readonly BitmapImage m_FailedImage;
        
        static BatchJobItemIssueTypeToImageConverter()
        {
            m_InfoImage = Resources.info.ToBitmapImage(true);
            m_WarningImage = Resources.status_warning.ToBitmapImage(true);
            m_FailedImage = Resources.status_failed.ToBitmapImage(true);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BatchJobItemIssueType_e)
            {
                switch ((BatchJobItemIssueType_e)value) 
                {
                    case BatchJobItemIssueType_e.Information:
                        return m_InfoImage;
                    case BatchJobItemIssueType_e.Warning:
                        return m_WarningImage;
                    case BatchJobItemIssueType_e.Error:
                        return m_FailedImage;
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
