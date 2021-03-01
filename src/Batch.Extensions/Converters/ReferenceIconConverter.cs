using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using Xarial.CadPlus.Plus.Services;
using Xarial.XCad.Documents;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Batch.Extensions.Converters
{
    public class ReferenceIconConverter : IMultiValueConverter
    {
        private enum DocumentType_e 
        {
            Part,
            Assembly,
            Drawing
        }

        private ImageSource m_PartIcon;
        private ImageSource m_AssemblyIcon;
        private ImageSource m_DrawingIcon;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var item = values[0];
            var cadDesc = (ICadEntityDescriptor)values[1];

            var docType = GetDocumentType(item, cadDesc);

            switch (docType) 
            {
                case DocumentType_e.Part:
                    return m_PartIcon ?? (m_PartIcon = cadDesc.PartIcon.ToBitmapImage());

                case DocumentType_e.Assembly:
                    return m_AssemblyIcon ?? (m_AssemblyIcon = cadDesc.AssemblyIcon.ToBitmapImage());

                case DocumentType_e.Drawing:
                    return m_DrawingIcon ?? (m_DrawingIcon = cadDesc.DrawingIcon.ToBitmapImage());

                default:
                    return null;
            }
        }

        private DocumentType_e? GetDocumentType(object item, ICadEntityDescriptor cadDesc) 
        {
            if (item is IXPart)
            {
                return DocumentType_e.Part;
            }
            else if (item is IXAssembly)
            {
                return DocumentType_e.Assembly;
            }
            else if (item is IXDrawing)
            {
                return DocumentType_e.Drawing;
            }
            else if (item is IXDocument)
            {
                if (MatchesExtension(item as IXDocument, cadDesc.PartFileFilter.Extensions))
                {
                    return DocumentType_e.Part;
                }
                else if (MatchesExtension(item as IXDocument, cadDesc.AssemblyFileFilter.Extensions)) 
                {
                    return DocumentType_e.Assembly;
                }
                else if (MatchesExtension(item as IXDocument, cadDesc.DrawingFileFilter.Extensions))
                {
                    return DocumentType_e.Drawing;
                }
            }

            return null;
        }

        private bool MatchesExtension(IXDocument doc, string[] exts)
        {
            try
            {
                var ext = Path.GetExtension(doc.Path);
                return exts.Any(e => Path.GetExtension(e).Equals(ext, StringComparison.CurrentCultureIgnoreCase));
            }
            catch 
            {
                return false;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
