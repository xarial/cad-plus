//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xarial.CadPlus.Batch.Base.Properties;
using Xarial.CadPlus.Batch.Base.Services;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Plus.Services;
using Xarial.XCad.Documents;
using Xarial.XCad.UI;
using Xarial.XToolkit;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Batch.Base.Core
{
    public class JobItemDocument : IJobItemDocument
    {
        private class CadObjectIcons
        {
            internal BitmapImage Part { get; set; }
            internal BitmapImage Assembly { get; set; }
            internal BitmapImage Drawing { get; set; }
        }

        public event BatchJobItemNestedItemsInitializedDelegate NestedItemsInitialized;

        IBatchJobItemState IBatchJobItem.State => State;
        IReadOnlyList<IBatchJobItemOperation> IBatchJobItem.Operations => Operations;

        private static readonly Dictionary<string, CadObjectIcons> m_Icons;
        private static BitmapImage m_DefaultIcon;

        static JobItemDocument()
        {
            m_Icons = new Dictionary<string, CadObjectIcons>();
        }

        public IXDocument Document { get; }

        public BitmapImage Icon
        {
            get
            {
                if (!m_Icons.TryGetValue(m_CadDesc.ApplicationId, out CadObjectIcons icons))
                {
                    icons = new CadObjectIcons();
                    m_Icons.Add(m_CadDesc.ApplicationId, icons);
                }

                if (TextUtils.MatchesAnyFilter(Document.Path, m_CadDesc.PartFileFilter.Extensions))
                {
                    return icons.Part ?? (icons.Part = m_CadDesc.PartIcon.ToBitmapImage(true));
                }
                else if (TextUtils.MatchesAnyFilter(Document.Path, m_CadDesc.AssemblyFileFilter.Extensions))
                {
                    return icons.Assembly ?? (icons.Assembly = m_CadDesc.AssemblyIcon.ToBitmapImage(true));
                }
                else if (TextUtils.MatchesAnyFilter(Document.Path, m_CadDesc.DrawingFileFilter.Extensions))
                {
                    return icons.Drawing ?? (icons.Drawing = m_CadDesc.DrawingIcon.ToBitmapImage(true));
                }

                return m_DefaultIcon ?? (m_DefaultIcon = Resources.file_icon.ToBitmapImage(true));
            }
        }

        public BitmapImage Preview
        {
            get
            {
                try
                {
                    if (!Document.IsCommitted || Document.IsAlive)
                    {
                        switch (Document)
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

        public string Title { get; }

        public string Description { get; }

        public Action Link { get; }

        public IReadOnlyList<JobItemMacro> Operations { get; }

        //TODO: implement support for configurations and sheets
        public IReadOnlyList<IBatchJobItem> Nested { get; }

        public JobItemState State { get; }

        private readonly ICadDescriptor m_CadDesc;
                
        public JobItemDocument(IXDocument doc, IReadOnlyList<JobItemMacro> macros, ICadDescriptor cadDesc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            m_CadDesc = cadDesc;

            State = new JobItemState();

            Document = doc;
            Title = Path.GetFileName(doc.Path);
            Description = doc.Path;
            Operations = macros;
            Link = TryOpenInExplorer;
        }

        private void TryOpenInExplorer()
        {
            try
            {
                FileSystemUtils.BrowseFileInExplorer(Document.Path);
            }
            catch 
            {
            }
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
                        return Image.FromStream(memStr).ToBitmapImage(true);
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
