//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using QRCoder;
using System;
using System.Drawing;
using System.IO;
using Xarial.CadPlus.Drawing.QrCode.Properties;
using Xarial.CadPlus.Drawing.QrCode.Services;
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
using Xarial.CadPlus.Drawing.QrCode.Data;
using System.ComponentModel;
using Xarial.CadPlus.Plus.Modules;
using Xarial.XCad.Base;
using Xarial.XCad.UI.Commands;
using Xarial.XCad.Base.Enums;
using SolidWorks.Interop.swconst;
using System.Linq;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XToolkit.Services;
using Xarial.CadPlus.Drawing.QrCode.Features;
using Xarial.CadPlus.Plus.DI;
using Xarial.XCad.Features;
using Xarial.XToolkit.Services.Expressions;
using System.Collections.Generic;
using Xarial.XCad.Sketch;

namespace Xarial.CadPlus.Drawing.QrCode
{
    [Title("Drawing+")]
    [IconEx(typeof(Resources), nameof(Resources.drawing_vector), nameof(Resources.drawing_icon))]
    [CommandGroupInfo((int)CadCommandGroupIds_e.Drawing)]
    [CommandOrder(3)]
    public enum Commands_e
    {
        [CommandItemInfo(true, true, WorkspaceTypes_e.Drawing, true)]
        [IconEx(typeof(Resources), nameof(Resources.qrcode_vector), nameof(Resources.qrcode_icon))]
        [Title("Insert QR Code")]
        [Description("Inserts QR code based on custom data source into the current drawing")]
        InsertQrCode = 0
    }

    [Title("Drawing+ QR Code")]
    [CommandGroupInfo((int)CadCommandGroupIds_e.QrCodeContextMenu)]
    public enum PictureContextMenuCommands_e
    {
        [Title("Edit")]
        [Description("Edits the QR code data of this QR code feature")]
        [CommandItemInfo(WorkspaceTypes_e.Drawing)]
        [IconEx(typeof(Resources), nameof(Resources.qr_code_edit_vector), nameof(Resources.qr_code_edit))]
        EditQrCode,

        [Title("Update")]
        [Description("Updates QR code in the current location preserving the current size")]
        [CommandItemInfo(WorkspaceTypes_e.Drawing)]
        [IconEx(typeof(Resources), nameof(Resources.qr_code_update_in_place_vector), nameof(Resources.qr_code_update_in_place))]
        UpdateQrCodeInPlace,

        [Title("Reload")]
        [Description("Reloads QR code and updates the location and size according to sheet scale")]
        [CommandItemInfo(WorkspaceTypes_e.Drawing)]
        [IconEx(typeof(Resources), nameof(Resources.qr_code_update_vector), nameof(Resources.qr_code_update))]
        Reload
    }

    //TODO: remove the dependency on application once the common APIs are used
    [Module(typeof(IHostCadExtension), typeof(ISwAddInApplication))]
    public class DrawingQrCodeModule : IDrawingQrCodeModule
    {
        private IHostCadExtension m_Host;

        private InsertQrCodeFeature m_InsertQrCodeFeature;
        private EditQrCodeFeature m_EditQrCodeFeature;

        private QrDataProvider m_DataProvider;

        private IServiceProvider m_SvcProvider;

        private IXLogger m_Logger;
        private IMessageService m_MsgSvc;
        private IExpressionParser m_Parser;

        public void Init(IHost host)
        {
            m_Host = (IHostCadExtension)host;
            m_Host.Initialized += OnHostInitialized;
            m_Host.Connect += OnConnect;
            m_Host.ConfigureServices += OnConfigureServices;
        }

        private void OnConfigureServices(IContainerBuilder builder)
        {
            builder.RegisterSelfSingleton<QrDataProvider>();
            builder.RegisterSelfSingleton<InsertQrCodeFeature>().UsingParameters(Parameter<ExpressionSolveErrorHandlerDelegate>.Any(ResolveExpressionError));
            builder.RegisterSelfSingleton<EditQrCodeFeature>().UsingParameters(Parameter<ExpressionSolveErrorHandlerDelegate>.Any(ResolveExpressionError));
        }

        private void OnHostInitialized(IApplication app, IServiceProvider svcProvider, IModule[] modules)
        {
            m_SvcProvider = svcProvider;

            m_Logger = m_SvcProvider.GetService<IXLogger>();
            m_MsgSvc = m_SvcProvider.GetService<IMessageService>();
            m_Parser = m_SvcProvider.GetService<IExpressionParser>();
            m_DataProvider = m_SvcProvider.GetService<QrDataProvider>();
        }

        private void OnConnect()
        {
            m_Host.Extension.CommandManager.AddContextMenu<PictureContextMenuCommands_e>(SelectType_e.SketchPicture).CommandClick += OnPictureContextMenuCommandClick;

            m_Host.RegisterCommands<Commands_e>(OnCommandClick);

            m_InsertQrCodeFeature = m_SvcProvider.GetService<InsertQrCodeFeature>();
            m_EditQrCodeFeature = m_SvcProvider.GetService<EditQrCodeFeature>();

            m_Host.Extension.Application.Documents.RegisterHandler(() => new QrCodeDrawingHandler(m_Parser, m_Logger));
        }

        private void OnCommandClick(Commands_e cmd)
        {
            switch (cmd)
            {
                case Commands_e.InsertQrCode:
                    var drw = (IXDrawing)m_Host.Extension.Application.Documents.Active;
                    m_InsertQrCodeFeature.Insert(new QrCodeElement(m_Host.Extension.Application, drw, drw.Sheets.Active, m_DataProvider, m_Logger));
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
                            var drw = (IXDrawing)m_Host.Extension.Application.Documents.Active;
                            var pict = GetSelectedPictures(drw).Last();
                            m_EditQrCodeFeature.Edit(QrCodeElement.FromSketchPicture(pict, drw, m_Host.Extension.Application, m_DataProvider, m_Logger));
                        }
                        break;

                    case PictureContextMenuCommands_e.UpdateQrCodeInPlace:
                        {
                            var drw = (ISwDrawing)m_Host.Extension.Application.Documents.Active;

                            foreach (var pict in GetSelectedPictures(drw))
                            {
                                var qrCodeElem = QrCodeElement.FromSketchPicture(pict, drw, m_Host.Extension.Application, m_DataProvider, m_Logger);
                                qrCodeElem.Update(ResolveExpressionError);
                            }
                        }
                        break;

                    case PictureContextMenuCommands_e.Reload:
                        {
                            var drw = (ISwDrawing)m_Host.Extension.Application.Documents.Active;

                            foreach (var pict in GetSelectedPictures(drw))
                            {
                                var qrCodeElem = QrCodeElement.FromSketchPicture(pict, drw, m_Host.Extension.Application, m_DataProvider, m_Logger);
                                qrCodeElem.Reload(ResolveExpressionError);
                            }
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

        private void ResolveExpressionError(Exception err, string expr, out bool cancel)
            => cancel = m_MsgSvc.ShowQuestion("Failed to resolve QR Code expression: " + err.ParseUserError() + Environment.NewLine + "Do you want to insert QR code placeholder without the data?") != true;

        private IXSketchPicture[] GetSelectedPictures(IXDrawing drw)
            => drw.Selections.OfType<IXSketchPicture>().ToArray();

        public IQrCodeElement GetQrCode(IXDrawing drw, IXSketchPicture qrCodePicture) 
            => QrCodeElement.FromSketchPicture(qrCodePicture, drw, m_Host.Extension.Application, m_DataProvider, m_Logger);
        
        public IEnumerable<IQrCodeElement> IterateQrCodes(IXDrawing drw)
        {
            var app = m_Host.Extension.Application;

            var handler = app.Documents.GetHandler<QrCodeDrawingHandler>(drw);
            
            foreach (var qrCodeInfo in handler.QrCodes) 
            {
                yield return new QrCodeElement(qrCodeInfo, app, drw, m_DataProvider, m_Logger);
            }
        }

        public IQrCodeElement Insert(IXDrawing drw, IXSheet sheet, QrCodeDock_e dock, double size, double offsetX, double offsetY, string expression, ExpressionSolveErrorHandlerDelegate errHandler)
        {
            var elem = new QrCodeElement(m_Host.Extension.Application, drw, sheet, m_DataProvider, m_Logger);

            elem.Create(dock, size, offsetX, offsetY, expression, errHandler);

            return elem;
        }

        public void Dispose()
        {
            m_InsertQrCodeFeature?.Dispose();
            m_EditQrCodeFeature?.Dispose();
        }
    }
}
