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
using Xarial.CadPlus.Drawing.Features;

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
        [IconEx(typeof(Resources), nameof(Resources.qr_code_edit_vector), nameof(Resources.qr_code_edit))]
        EditQrCode,

        [Title("Update")]
        [CommandItemInfo(WorkspaceTypes_e.Drawing)]
        [IconEx(typeof(Resources), nameof(Resources.qr_code_update_vector), nameof(Resources.qr_code_update))]
        UpdateQrCode
    }

    //TODO: remove the dependency on application once the common APIs are used
    [Module(typeof(IHostExtension), typeof(ISwAddInApplication))]
    public class DrawingModule : IModule
    {
        private IHostExtension m_Host;

        private IXPropertyPage<EditQrCodeData> m_EditQrCodePage;

        private IInsertQrCodeFeature m_InsertQrCodeFeature;
        private IEditQrCodeFeature m_EditQrCodeFeature;

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

            m_InsertQrCodeFeature = new InsertQrCodeFeature(m_Host.Extension, m_MsgSvc, m_Logger);
            m_EditQrCodeFeature = new EditQrCodeFeature(m_Host.Extension, m_MsgSvc, m_Logger);

            m_SettsProvider = m_SvcProvider.GetService<ISettingsProvider>();

            m_Host.Extension.Application.Documents.RegisterHandler(() => new QrCodeDrawingHandler(m_Logger));
        }

        private void OnCommandClick(Commands_e cmd) 
        {
            switch (cmd) 
            {
                case Commands_e.InsertQrCode:
                    m_InsertQrCodeFeature.Insert((IXDrawing)m_Host.Extension.Application.Documents.Active);
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
                        {
                            var drw = (ISwDrawing)m_Host.Extension.Application.Documents.Active;
                            var pict = GetSelectedPicture(drw);
                            m_EditQrCodeFeature.Edit(pict, drw);
                        }
                        break;

                    case PictureContextMenuCommands_e.UpdateQrCode:
                        {
                            var drw = (ISwDrawing)m_Host.Extension.Application.Documents.Active;
                            var pict = GetSelectedPicture(drw);
                            m_EditQrCodeFeature.Update(pict, drw);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
                m_MsgSvc.ShowError(ex);
            }
        }

        private ISwObject GetSelectedPicture(ISwDrawing drw)
        {   
            var pict = (ISwObject)drw.Selections.Last();

            if (pict is ISwFeature)
            {
                pict = SwObjectFactory.FromDispatch<ISwObject>(
                    ((ISwFeature)pict).Feature.GetSpecificFeature2(), drw);
            }

            return pict;
        }

        public void Dispose()
        {
            m_InsertQrCodeFeature.Dispose();
        }
    }
}
