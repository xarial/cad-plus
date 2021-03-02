//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
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
using System.Windows.Media;
using Xarial.CadPlus.Plus.Services;
using Xarial.XCad.Documents;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Batch.InApp.Converters
{
    public class ReferenceIconConverter : IMultiValueConverter
    {
        private ImageSource m_PartIcon;
        private ImageSource m_AssemblyIcon;
        private ImageSource m_DrawingIcon;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var item = values[0];
            var cadDesc = (ICadEntityDescriptor)values[1];

            if (item is IXPart)
            {
                return m_PartIcon ?? (m_PartIcon = cadDesc.PartIcon.ToBitmapImage());
            }
            else if (item is IXAssembly)
            {
                return m_AssemblyIcon ?? (m_AssemblyIcon = cadDesc.AssemblyIcon.ToBitmapImage());
            }
            else if (item is IXDrawing)
            {
                return m_DrawingIcon ?? (m_DrawingIcon = cadDesc.DrawingIcon.ToBitmapImage());
            }
            else 
            {
                return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
