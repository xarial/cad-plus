//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using QRCoder;
using System;
using System.Drawing;
using System.IO;
using Xarial.CadPlus.Drawing.Properties;
using Xarial.CadPlus.Drawing.Services;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.CadPlus.Plus.Services;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Documents;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.UI.Commands.Attributes;
using Xarial.XCad.UI.Commands.Enums;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.UI.PropertyPage.Structures;
using Xarial.XToolkit.Reporting;
using Xarial.CadPlus.Plus.Extensions;
using Xarial.CadPlus.Drawing.Data;
using System.ComponentModel;
using Xarial.CadPlus.Plus.Modules;
using Xarial.XCad.Base;
using Xarial.XCad.UI.Commands;
using Xarial.XCad.Base.Enums;
using SolidWorks.Interop.swconst;
using System.Linq;
using Xarial.XCad.SolidWorks.Features;

namespace Xarial.CadPlus.Drawing
{
    [Title("Drawing+")]
    [IconEx(typeof(Resources), nameof(Resources.drawing_vector), nameof(Resources.drawing_icon))]
    public enum Commands_e 
    {
        [CommandItemInfo(WorkspaceTypes_e.Drawing)]
        [IconEx(typeof(Resources), nameof(Resources.qrcode_vector), nameof(Resources.qrcode_icon))]
        [Title("Insert QR Code")]
        [Description("Inserts QR code based on custom data source into the current drawing")]
        InsertQrCode
    }

    [CommandGroupInfo(2000)]
    [Title("Drawing+ QR Code")]
    public enum PictureContextMenuCommands_e 
    {
        [Title("Edit")]
        [CommandItemInfo(WorkspaceTypes_e.Drawing)]
        EditQrCode,

        [Title("Update")]
        [CommandItemInfo(WorkspaceTypes_e.Drawing)]
        UpdateQrCode
    }

    //TODO: remove the dependency on application once the common APIs are used
    [Module(typeof(IHostExtension), typeof(ISwAddInApplication))]
    public class DrawingModule : IModule
    {
        private IHostExtension m_Host;

        private IXPropertyPage<InsertQrCodeData> m_Page;
        
        private QrDataProvider m_QrDataProvider;
        private QrCodeManager m_QrCodeManager;

        private InsertQrCodeData m_CurPageData;
        private QrCodePreviewer m_CurPreviewer;
        private IXDrawing m_CurDrawing;
        
        private ISettingsProvider m_SettsProvider;
        private IServiceProvider m_SvcProvider;

        private IXLogger m_Logger;
        private IMessageService m_MsgSvc;

        public void Init(IHost host)
        {
            m_Host = (IHostExtension)host;
            m_Host.Initialized += OnHostInitialized;
            m_Host.Connect += OnConnect;
        }

        private void OnHostInitialized(IApplication app, IServiceContainer svcProvider, IModule[] modules)
        {
            m_SvcProvider = svcProvider;

            m_Logger = m_SvcProvider.GetService<IXLogger>();
            m_MsgSvc = m_SvcProvider.GetService<IMessageService>();
        }

        private void OnConnect()
        {
            m_Host.Extension.CommandManager.AddContextMenu<PictureContextMenuCommands_e>(
                (SelectType_e)swSelectType_e.swSelSKETCHBITMAP).CommandClick += OnPictureContextMenuCommandClick;

            m_Host.RegisterCommands<Commands_e>(OnCommandClick);
            m_Page = m_Host.Extension.CreatePage<InsertQrCodeData>();
            m_CurPageData = new InsertQrCodeData();

            m_SettsProvider = m_SvcProvider.GetService<ISettingsProvider>();

            m_QrDataProvider = new QrDataProvider(m_Host.Extension.Application);
            m_QrCodeManager = new QrCodeManager(m_Host.Extension.Application, m_QrDataProvider);

            m_Page.DataChanged += OnPageDataChanged;
            m_Page.Closed += OnPageClosed;
            m_Page.Closing += OnPageClosing;

            m_Host.Extension.Application.Documents.RegisterHandler(() => new QrCodeDrawingHandler(m_Logger));
        }

        private void OnPageClosing(PageCloseReasons_e reason, PageClosingArg arg)
        {
            if (reason == PageCloseReasons_e.Okay) 
            {
                try
                {
                    if (string.IsNullOrEmpty(m_QrDataProvider.GetData(m_CurDrawing, m_CurPageData.Source))) 
                    {
                        throw new UserException("Data for QR code is empty");
                    }
                }
                catch (Exception ex)
                {
                    m_Logger.Log(ex);
                    arg.Cancel = true;
                    arg.ErrorMessage = ex.ParseUserError(out _);
                }
            }
        }

        private void OnPageClosed(PageCloseReasons_e reason)
        {
            m_CurPreviewer.Dispose();
            m_CurPreviewer = null;

            if (reason == PageCloseReasons_e.Okay) 
            {
                try
                {
                    var pict = m_QrCodeManager.Insert(m_CurDrawing, m_CurPageData.Location, m_CurPageData.Source);
                    var handler = m_Host.Extension.Application.Documents.GetHandler<QrCodeDrawingHandler>(m_CurDrawing);

                    var arg = "";
                    switch (m_CurPageData.Source.Source) 
                    {
                        case Source_e.CustomProperty:
                            arg = m_CurPageData.Source.CustomPropertyName;
                            break;

                        case Source_e.PdmWeb2Url:
                            arg = m_CurPageData.Source.PdmWeb2Server;
                            break;

                        case Source_e.Custom:
                            arg = m_CurPageData.Source.CustomValue;
                            break;
                    }

                    handler.QrCodes.Add(new QrCodeData()
                    {
                        Picture = pict,
                        Dock = m_CurPageData.Location.Dock,
                        Source = m_CurPageData.Source.Source,
                        RefDocumentSource = m_CurPageData.Source.ReferencedDocument,
                        Argument = arg
                    });
                }
                catch (Exception ex)
                {
                    m_Logger.Log(ex);
                    m_MsgSvc.ShowError(ex);
                }
            }
        }

        private void OnPageDataChanged()
        {
            UpdatePreview();
        }
        
        private void UpdatePreview()
        {
            m_CurPreviewer.Preview(
                m_CurPageData.Location.Dock,
                m_CurPageData.Location.Size,
                m_CurPageData.Location.OffsetX,
                m_CurPageData.Location.OffsetY);
        }

        private void OnCommandClick(Commands_e cmd) 
        {
            switch (cmd) 
            {
                case Commands_e.InsertQrCode:
                    m_CurDrawing = (IXDrawing)m_Host.Extension.Application.Documents.Active;
                    m_CurPreviewer = new QrCodePreviewer(m_CurDrawing, m_QrCodeManager);
                    m_Page.Show(m_CurPageData);
                    UpdatePreview();
                    break;
            }
        }

        private void OnPictureContextMenuCommandClick(PictureContextMenuCommands_e spec)
        {
            try
            {
                switch (spec)
                {
                    case PictureContextMenuCommands_e.EditQrCode:
                        break;

                    case PictureContextMenuCommands_e.UpdateQrCode:
                        var drw = (ISwDrawing)m_Host.Extension.Application.Documents.Active;
                        var pict = (ISwObject)drw.Selections.Last();
                        
                        if (pict is ISwFeature) 
                        {
                            pict = SwObjectFactory.FromDispatch<ISwObject>(
                                ((ISwFeature)pict).Feature.GetSpecificFeature2(), drw);
                        }

                        m_QrCodeManager.Update(pict, drw);
                        break;
                }
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
                m_MsgSvc.ShowError(ex);
            }
        }

        public void Dispose()
        {
            m_CurPreviewer?.Dispose();
        }
    }
}
