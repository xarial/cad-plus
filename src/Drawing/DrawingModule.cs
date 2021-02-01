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

namespace Xarial.CadPlus.Drawing
{
    [Title("Drawing+")]
    public enum Commands_e 
    {
        [CommandItemInfo(WorkspaceTypes_e.Drawing)]
        [IconEx(typeof(Resources), nameof(Resources.qrcode_vector), nameof(Resources.qrcode_icon))]
        [Title("Insert QR Code")]
        [Description("Inserts QR code based on custom data source into the current drawing")]
        InsertQrCode
    }

    //TODO: remove the dependency on application once the common APIs are used
    [Module(typeof(IHostExtension), ApplicationIds.SolidWorksAddIn)]
    public class DrawingModule : IModule
    {
        public Guid Id => Guid.Parse(ModuleIds.Drawing);

        private IHostExtension m_Host;

        private IXPropertyPage<InsertQrCodeData> m_Page;
        
        private QrDataProvider m_QrDataProvider;

        private InsertQrCodeData m_CurPageData;
        private QrCodePreviewer m_CurPreviewer;
        private IXDrawing m_CurDrawing;
        private string m_CurQrCodeData;
        private ISettingsProvider m_SettsProvider;

        public void Init(IHost host)
        {
            m_Host = (IHostExtension)host;
            m_Host.Connect += OnConnect;
        }

        private void OnConnect()
        {
            m_Host.RegisterCommands<Commands_e>(OnCommandClick);
            m_Page = m_Host.CreatePage<InsertQrCodeData>();
            m_CurPageData = new InsertQrCodeData();

            m_SettsProvider = m_Host.Services.GetService<ISettingsProvider>();

            m_QrDataProvider = new QrDataProvider(m_Host.Extension.Application,
                m_SettsProvider.ReadSettings<DrawingSettings>());

            m_Page.DataChanged += OnPageDataChanged;
            m_Page.Closed += OnPageClosed;
            m_Page.Closing += OnPageClosing;
        }

        private void OnPageClosing(PageCloseReasons_e reason, PageClosingArg arg)
        {
            if (reason == PageCloseReasons_e.Okay) 
            {
                try
                {
                    m_CurQrCodeData = m_QrDataProvider.GetData(m_CurDrawing, m_CurPageData.Source);

                    if (string.IsNullOrEmpty(m_CurQrCodeData)) 
                    {
                        throw new UserException("Data for QR code is empty");
                    }
                }
                catch (Exception ex)
                {
                    arg.Cancel = true;
                    arg.ErrorMessage = ex.ParseUserError(out _);
                }
            }
        }

        private void OnPageClosed(PageCloseReasons_e reason)
        {
            m_CurPreviewer.Dispose();

            if (reason == PageCloseReasons_e.Okay) 
            {
                try
                {
                    InsertQrCode();
                }
                catch 
                {
                    //TODO: show error message
                }
            }
        }

        private void OnPageDataChanged()
        {
            UpdatePreview();
        }

        private void InsertQrCode() 
        {
            var tempFileName = "";

            var model = (m_CurDrawing as ISwDrawing).Model;

            try
            {
                var qrGenerator = new QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode(m_CurQrCodeData,
                    QRCodeGenerator.ECCLevel.Q);
                var qrCode = new QRCode(qrCodeData);
                var qrCodeImage = qrCode.GetGraphic(20, Color.Black, Color.White, false);

                tempFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".png");
                qrCodeImage.Save(tempFileName);

                var pict = model.SketchManager.InsertSketchPicture(tempFileName);
                
                if (pict != null)
                {
                    m_CurPreviewer.CalculateLocation(m_CurDrawing, m_CurPageData.Location.Dock, 
                        m_CurPageData.Location.Size, m_CurPageData.Location.OffsetX, 
                        m_CurPageData.Location.OffsetY, 
                        out XCad.Geometry.Structures.Point centerPt, out double scale);

                    var x = centerPt.X / scale - m_CurPageData.Location.Size / 2;
                    var y = centerPt.Y / scale - m_CurPageData.Location.Size / 2;
                    pict.SetOrigin(x, y);
                    pict.SetSize(m_CurPageData.Location.Size, m_CurPageData.Location.Size, true);

                    //Picture PMPage stays open after inserting the picture
                    const int swCommands_PmOK = -2;
                    (m_Host.Extension.Application as ISwApplication).Sw.RunCommand(swCommands_PmOK, "");
                }
                else 
                {
                    throw new UserException("Failed to insert picture");
                }
            }
            finally
            {
                model.IActiveView.EnableGraphicsUpdate = true;

                if (File.Exists(tempFileName)) 
                {
                    try
                    {
                        File.Delete(tempFileName);
                    }
                    catch 
                    {
                    }
                }
            }
        }

        private void UpdatePreview()
        {
            m_CurPreviewer.Preview(null,
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
                    m_CurPreviewer = new QrCodePreviewer(m_CurDrawing);
                    m_Page.Show(m_CurPageData);
                    UpdatePreview();
                    break;
            }
        }


        public void Dispose()
        {
        }
    }
}
