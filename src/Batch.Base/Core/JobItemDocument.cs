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
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xarial.CadPlus.Batch.Base.Properties;
using Xarial.CadPlus.Batch.Base.Services;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Shared.Extensions;
using Xarial.CadPlus.Plus.Shared.Helpers;
using Xarial.CadPlus.Plus.Shared.Services;
using Xarial.XCad.Documents;
using Xarial.XCad.UI;
using Xarial.XToolkit;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Batch.Base.Core
{
    public class JobItemDocument : IJobItemDocument
    {
        public event BatchJobItemNestedItemsInitializedDelegate NestedItemsInitialized;

        IBatchJobItemState IBatchJobItem.State => State;
        IReadOnlyList<IBatchJobItemOperation> IBatchJobItem.Operations => Operations;

        public IXDocument Document { get; }

        public BitmapImage Icon { get; }

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
                                return doc3D.Configurations.Active.Preview?.TryConvertImage();

                            case IXDrawing drw:
                                return drw.Sheets.Active.Preview.TryConvertImage();
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

        public BatchJobItemState State { get; }

        private readonly ICadDescriptor m_CadDesc;
                
        public JobItemDocument(IXDocument doc, IReadOnlyList<JobItemOperationMacroDefinition> macrosDefs, ICadDescriptor cadDesc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            m_CadDesc = cadDesc;

            State = new BatchJobItemState(this);

            Document = doc;

            Title = Path.GetFileName(doc.Path);
            Description = doc.Path;
            Icon = CadObjectIconStore.Instance.GetIcon(doc, m_CadDesc);

            Operations = macrosDefs.Select(m => new JobItemMacro(this, m)).ToArray();
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
    }
}
