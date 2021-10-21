//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xarial.CadPlus.Batch.Base.Core;
using Xarial.CadPlus.Batch.Base.Properties;
using Xarial.CadPlus.Batch.Base.ViewModels;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Shared.Controls;
using Xarial.XCad.Documents;
using Xarial.XCad.UI;
using Xarial.XToolkit;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Batch.Base.ViewModels
{
    public class JobItemCadObject : ICustomObject
    {
        private class CadObjectIcons
        {
            internal ImageSource Part { get; set; }
            internal ImageSource Assembly { get; set; }
            internal ImageSource Drawing { get; set; }
        }

        private static readonly Dictionary<string, CadObjectIcons> m_Icons;
        private static ImageSource m_DefaultIcon;

        static JobItemCadObject() 
        {
            m_Icons = new Dictionary<string, CadObjectIcons>();
        }

        public string Title => System.IO.Path.GetFileName(Path);

        public string Tooltip => Path;

        public string Path { get; }

        public ImageSource Preview 
        {
            get
            {
                try
                {
                    if (!m_Doc.IsCommitted || m_Doc.IsAlive)
                    {
                        switch (m_Doc)
                        {
                            case IXDocument3D doc3D:
                                return ConvertImage(doc3D.Configurations.Active.Preview);

                            case IXDrawing drw:
                                return ConvertImage(drw.Sheets.Active.Preview);
                        }
                    }
                }
                catch 
                {
                }

                return null;
            }
        }

        public ImageSource Icon 
        {
            get
            {
                if (!m_Icons.TryGetValue(m_CadDesc.ApplicationId, out CadObjectIcons icons))
                {
                    icons = new CadObjectIcons();
                    m_Icons.Add(m_CadDesc.ApplicationId, icons);
                }

                if (TextUtils.MatchesAnyFilter(Path, m_CadDesc.PartFileFilter.Extensions))
                {
                    return icons.Part ?? (icons.Part = m_CadDesc.PartIcon.ToBitmapImage());
                }
                else if (TextUtils.MatchesAnyFilter(Path, m_CadDesc.AssemblyFileFilter.Extensions))
                {
                    return icons.Assembly ?? (icons.Assembly = m_CadDesc.AssemblyIcon.ToBitmapImage());
                }
                else if (TextUtils.MatchesAnyFilter(Path, m_CadDesc.DrawingFileFilter.Extensions))
                {
                    return icons.Drawing ?? (icons.Drawing = m_CadDesc.DrawingIcon.ToBitmapImage());
                }

                return m_DefaultIcon ?? (m_DefaultIcon = Resources.file_icon.ToBitmapImage());
            }
        }

        public bool CanOpenInExplorer => true;

        private readonly IXDocument m_Doc;
        private readonly ICadDescriptor m_CadDesc;

        public JobItemCadObject(IXDocument doc, string filePath, ICadDescriptor cadDesc) 
        {
            m_Doc = doc;
            Path = filePath;
            m_CadDesc = cadDesc;
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

    public class JobItemDocumentVM : JobItemVM
    {
        private readonly JobItemDocument m_JobItemFile;

        public JobItemMacroVM[] Macros { get; }

        public JobItemCadObject DisplayObject { get; }

        public JobItemDocumentVM(JobItemDocument jobItemFile, ICadDescriptor cadDesc) : base(jobItemFile)
        {
            m_JobItemFile = jobItemFile;
            Macros = m_JobItemFile.Operations.Select(o => new JobItemMacroVM(o)).ToArray();
            DisplayObject = new JobItemCadObject(jobItemFile.Document, jobItemFile.FilePath, cadDesc);
        }
    }
}
