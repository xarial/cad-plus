//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xarial.CadPlus.Plus.Shared.Controls;
using Xarial.CadPlus.Plus.Shared.Extensions;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;
using Xarial.XCad.UI;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Plus.Shared.Converters
{
    public class CadObjectPreviewConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                switch (value)
                {
                    case IXDocument3D doc:
                        return doc.Configurations.Active.Preview.TryConvertImage();

                    case IXDrawing drw:
                        return drw.Sheets.Active.Preview.TryConvertImage();

                    case IXConfiguration conf:
                        return conf.Preview.TryConvertImage();

                    case IXSheet sheet:
                        return sheet.Preview.TryConvertImage();

                    case IXCutListItem cutList:
                        //no preview
                        break;
                }
            }
            catch
            {
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
