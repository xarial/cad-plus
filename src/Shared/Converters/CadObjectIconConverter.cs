//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Shared.Controls;
using Xarial.CadPlus.Plus.Shared.Helpers;
using Xarial.CadPlus.Plus.Shared.Properties;
using Xarial.XCad;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Plus.Shared.Converters
{
    public class CadObjectIconConverter : IMultiValueConverter
    {
        private static BitmapImage m_DefaultIcon;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var ent = values[0];
                var cadDesc = (ICadDescriptor)values[1];

                if (ent is IXObject)
                {
                    return CadObjectIconStore.Instance.GetIcon((IXObject)ent, cadDesc);
                }
            }
            catch 
            {
            }

            return m_DefaultIcon ?? (m_DefaultIcon = Resources.file_icon.ToBitmapImage(true));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
