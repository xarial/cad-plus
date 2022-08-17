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
                        return ConvertImage(doc.Configurations.Active.Preview);

                    case IXDrawing drw:
                        return ConvertImage(drw.Sheets.Active.Preview);

                    case IXConfiguration conf:
                        return ConvertImage(conf.Preview);

                    case IXSheet sheet:
                        return ConvertImage(sheet.Preview);

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

        private BitmapImage ConvertImage(IXImage img) 
        {
            try
            {
                if (img != null && img.Buffer != null)
                {
                    using (var memStr = new MemoryStream(img.Buffer))
                    {
                        memStr.Seek(0, SeekOrigin.Begin);
                        return Image.FromStream(memStr).ToBitmapImage();
                    }
                }
                else
                {
                    return null;
                }
            }
            catch 
            {
                return null;
            }
        }
    }
}
