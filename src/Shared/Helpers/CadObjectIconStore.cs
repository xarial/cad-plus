using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Shared.Controls;
using Xarial.CadPlus.Plus.Shared.Properties;
using Xarial.XCad;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Plus.Shared.Helpers
{
    public class CadObjectIconStore
    {
        private class CadObjectIcons
        {
            internal ImageSource Part { get; set; }
            internal ImageSource Assembly { get; set; }
            internal ImageSource Drawing { get; set; }
            internal ImageSource CutList { get; set; }
            internal ImageSource Configuration { get; set; }
        }

        public static CadObjectIconStore Instance { get; }

        static CadObjectIconStore()
        {
            Instance = new CadObjectIconStore();
        }

        private readonly Dictionary<string, CadObjectIcons> m_Icons;

        private ImageSource m_DefaultIcon;

        public CadObjectIconStore()
        {
            m_Icons = new Dictionary<string, CadObjectIcons>();
        }

        public ImageSource GetIcon(IXObject obj, ICadDescriptor cadDesc)
        {
            try
            {
                if (obj != null && cadDesc != null)
                {
                    if (!m_Icons.TryGetValue(cadDesc.ApplicationId, out CadObjectIcons icons))
                    {
                        icons = new CadObjectIcons();
                        m_Icons.Add(cadDesc.ApplicationId, icons);
                    }

                    if (obj is IXPart)
                    {
                        return icons.Part ?? (icons.Part = cadDesc.PartIcon.ToBitmapImage());
                    }
                    else if (obj is IXAssembly)
                    {
                        return icons.Assembly ?? (icons.Assembly = cadDesc.AssemblyIcon.ToBitmapImage());
                    }
                    else if (obj is IXDrawing)
                    {
                        return icons.Drawing ?? (icons.Drawing = cadDesc.DrawingIcon.ToBitmapImage());
                    }
                    else if (obj is IXDocument)
                    {
                        if (MatchesExtension(obj as IXDocument, cadDesc.PartFileFilter.Extensions))
                        {
                            return icons.Part ?? (icons.Part = cadDesc.PartIcon.ToBitmapImage());
                        }
                        else if (MatchesExtension(obj as IXDocument, cadDesc.AssemblyFileFilter.Extensions))
                        {
                            return icons.Assembly ?? (icons.Assembly = cadDesc.AssemblyIcon.ToBitmapImage());
                        }
                        else if (MatchesExtension(obj as IXDocument, cadDesc.DrawingFileFilter.Extensions))
                        {
                            return icons.Drawing ?? (icons.Drawing = cadDesc.DrawingIcon.ToBitmapImage());
                        }
                    }
                    else if (obj is IXConfiguration)
                    {
                        return icons.Configuration ?? (icons.Configuration = cadDesc.ConfigurationIcon.ToBitmapImage());
                    }
                    else if (obj is IXCutListItem)
                    {
                        return icons.CutList ?? (icons.CutList = cadDesc.CutListIcon.ToBitmapImage());
                    }
                    //else if(obj is IXSheet)
                }
            }
            catch
            {
            }

            return m_DefaultIcon ?? (m_DefaultIcon = Resources.file_icon.ToBitmapImage());
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
