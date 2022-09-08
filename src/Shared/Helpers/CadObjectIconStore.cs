﻿//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Shared.Controls;
using Xarial.CadPlus.Plus.Shared.Properties;
using Xarial.XCad;
using Xarial.XCad.Documents;
using Xarial.XCad.Enums;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Plus.Shared.Helpers
{
    public class CadObjectIconStore
    {
        private class CadObjectIcons
        {
            internal BitmapImage Part { get; set; }
            internal BitmapImage Assembly { get; set; }
            internal BitmapImage Drawing { get; set; }
            
            internal BitmapImage CutListSolidBody { get; set; }
            internal BitmapImage CutListSheetMetal { get; set; }
            internal BitmapImage CutListWeldment { get; set; }

            internal BitmapImage SolidBody { get; set; }
            internal BitmapImage SheetBody { get; set; }

            internal BitmapImage Configuration { get; set; }
            internal BitmapImage Sheet { get; set; }
        }

        public static CadObjectIconStore Instance { get; }

        static CadObjectIconStore()
        {
            Instance = new CadObjectIconStore();
        }

        private readonly Dictionary<string, CadObjectIcons> m_Icons;

        private BitmapImage m_DefaultIcon;

        public CadObjectIconStore()
        {
            m_Icons = new Dictionary<string, CadObjectIcons>();
        }

        public BitmapImage GetIcon(IXObject obj, ICadDescriptor cadDesc)
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

                    if (obj is IXPart || obj is IXPartComponent)
                    {
                        return icons.Part ?? (icons.Part = cadDesc.PartIcon.ToBitmapImage(true));
                    }
                    else if (obj is IXAssembly || obj is IXAssemblyComponent)
                    {
                        return icons.Assembly ?? (icons.Assembly = cadDesc.AssemblyIcon.ToBitmapImage(true));
                    }
                    else if (obj is IXDrawing)
                    {
                        return icons.Drawing ?? (icons.Drawing = cadDesc.DrawingIcon.ToBitmapImage(true));
                    }
                    else if (obj is IXDocument)
                    {
                        if (MatchesExtension(obj as IXDocument, cadDesc.PartFileFilter.Extensions))
                        {
                            return icons.Part ?? (icons.Part = cadDesc.PartIcon.ToBitmapImage(true));
                        }
                        else if (MatchesExtension(obj as IXDocument, cadDesc.AssemblyFileFilter.Extensions))
                        {
                            return icons.Assembly ?? (icons.Assembly = cadDesc.AssemblyIcon.ToBitmapImage(true));
                        }
                        else if (MatchesExtension(obj as IXDocument, cadDesc.DrawingFileFilter.Extensions))
                        {
                            return icons.Drawing ?? (icons.Drawing = cadDesc.DrawingIcon.ToBitmapImage(true));
                        }
                    }
                    else if (obj is IXConfiguration)
                    {
                        return icons.Configuration ?? (icons.Configuration = cadDesc.ConfigurationIcon.ToBitmapImage(true));
                    }
                    else if (obj is IXCutListItem)
                    {
                        switch (((IXCutListItem)obj).Type)
                        {
                            case CutListType_e.SheetMetal:
                                return icons.CutListSheetMetal ?? (icons.CutListSheetMetal = cadDesc.CutListSheetMetalIcon.ToBitmapImage(true));

                            case CutListType_e.Weldment:
                                return icons.CutListWeldment ?? (icons.CutListWeldment = cadDesc.CutListWeldmentIcon.ToBitmapImage(true));

                            case CutListType_e.SolidBody:
                                return icons.CutListSolidBody ?? (icons.CutListSolidBody = cadDesc.CutListSolidBodyIcon.ToBitmapImage(true));
                        }
                        
                    }
                    else if (obj is IXSheet)
                    {
                        return icons.Sheet ?? (icons.Sheet = cadDesc.SheetIcon.ToBitmapImage(true));
                    }
                    else if (obj is IXSolidBody) 
                    {
                        return icons.SolidBody ?? (icons.SolidBody = cadDesc.SolidBodyIcon.ToBitmapImage(true));
                    }
                    else if (obj is IXSheetBody)
                    {
                        return icons.SheetBody ?? (icons.SheetBody = cadDesc.SheetBodyIcon.ToBitmapImage(true));
                    }
                }
            }
            catch
            {
            }

            return m_DefaultIcon ?? (m_DefaultIcon = Resources.file_icon.ToBitmapImage(true));
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