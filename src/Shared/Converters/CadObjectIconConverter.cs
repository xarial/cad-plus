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
using Xarial.CadPlus.Plus.Shared.Properties;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Plus.Shared.Converters
{
    public class CadObjectIconConverter : IMultiValueConverter
    {
        private class CadObjectIcons 
        {
            internal ImageSource Part { get; set; }
            internal ImageSource Assembly { get; set; }
            internal ImageSource Drawing { get; set; }
            internal ImageSource CutList { get; set; }
            internal ImageSource Configuration { get; set; }
        }

        private static readonly Dictionary<string, CadObjectIcons> m_Icons;

        private static ImageSource m_DefaultIcon;

        static CadObjectIconConverter() 
        {
            m_Icons = new Dictionary<string, CadObjectIcons>();
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var ent = values[0];
                var cadDesc = (ICadDescriptor)values[1];

                if (ent != null && cadDesc != null)
                {
                    if (!m_Icons.TryGetValue(cadDesc.ApplicationId, out CadObjectIcons icons))
                    {
                        icons = new CadObjectIcons();
                        m_Icons.Add(cadDesc.ApplicationId, icons);
                    }

                    if (ent is IXPart)
                    {
                        return icons.Part ?? (icons.Part = cadDesc.PartIcon.ToBitmapImage());
                    }
                    else if (ent is IXAssembly)
                    {
                        return icons.Assembly ?? (icons.Assembly = cadDesc.AssemblyIcon.ToBitmapImage());
                    }
                    else if (ent is IXDrawing)
                    {
                        return icons.Drawing ?? (icons.Drawing = cadDesc.DrawingIcon.ToBitmapImage());
                    }
                    else if (ent is IXDocument) 
                    {
                        if (MatchesExtension(ent as IXDocument, cadDesc.PartFileFilter.Extensions))
                        {
                            return icons.Part ?? (icons.Part = cadDesc.PartIcon.ToBitmapImage());
                        }
                        else if (MatchesExtension(ent as IXDocument, cadDesc.AssemblyFileFilter.Extensions))
                        {
                            return icons.Assembly ?? (icons.Assembly = cadDesc.AssemblyIcon.ToBitmapImage());
                        }
                        else if (MatchesExtension(ent as IXDocument, cadDesc.DrawingFileFilter.Extensions))
                        {
                            return icons.Drawing ?? (icons.Drawing = cadDesc.DrawingIcon.ToBitmapImage());
                        }
                    }
                    else if (ent is IXConfiguration)
                    {
                        return icons.Configuration ?? (icons.Configuration = cadDesc.ConfigurationIcon.ToBitmapImage());
                    }
                    else if (ent is IXCutListItem)
                    {
                        return icons.CutList ?? (icons.CutList = cadDesc.CutListIcon.ToBitmapImage());
                    }
                }
            }
            catch 
            {
            }

            return m_DefaultIcon ?? (m_DefaultIcon = Resources.file_icon.ToBitmapImage());
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
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
    }
}
