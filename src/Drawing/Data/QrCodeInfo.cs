//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad;
using Xarial.XCad.Features;
using Xarial.XToolkit.Services.UserSettings;
using Xarial.XToolkit.Services.UserSettings.Attributes;

namespace Xarial.CadPlus.Drawing.Data
{
    public class QrCodeInfoVersionTransformer : BaseUserSettingsVersionsTransformer
    {
        public QrCodeInfoVersionTransformer()
        {
        }
    }

    [UserSettingVersion("1.0", typeof(QrCodeInfoVersionTransformer))]
    public class QrCodeInfo
    {
        public event Action<QrCodeInfo> Changed;

        private IXObject m_Picture;
        private Source_e m_Source;
        private string m_Argument;
        private bool m_RefDocumentSource;
        private double m_Size;
        private Dock_e m_Dock;
        private double m_OffsetX;
        private double m_OffsetY;

        public IXObject Picture 
        {
            get => m_Picture;
            set 
            {
                m_Picture = value;
                this.Changed?.Invoke(this);
            }
        }

        public Source_e Source
        {
            get => m_Source;
            set
            {
                m_Source = value;
                this.Changed?.Invoke(this);
            }
        }

        public bool RefDocumentSource 
        {
            get => m_RefDocumentSource;
            set 
            {
                m_RefDocumentSource = value;
                this.Changed?.Invoke(this);
            }
        }

        public string Argument
        {
            get => m_Argument;
            set
            {
                m_Argument = value;
                this.Changed?.Invoke(this);
            }
        }

        public double Size
        {
            get => m_Size;
            set
            {
                m_Size = value;
                this.Changed?.Invoke(this);
            }
        }

        public Dock_e Dock
        {
            get => m_Dock;
            set
            {
                m_Dock = value;
                this.Changed?.Invoke(this);
            }
        }

        public double OffsetX
        {
            get => m_OffsetX;
            set
            {
                m_OffsetX = value;
                this.Changed?.Invoke(this);
            }
        }

        public double OffsetY
        {
            get => m_OffsetY;
            set
            {
                m_OffsetY = value;
                this.Changed?.Invoke(this);
            }
        }

        public QrCodeData ToData()
            => new QrCodeData()
            {
                Source = new SourceData() 
                {
                    Source = Source,
                    ReferencedDocument = RefDocumentSource,
                    CustomPropertyName = Source == Source_e.CustomProperty ? Argument : "",
                    PdmWeb2Server = Source == Source_e.PdmWeb2Url ? Argument : "",
                    CustomValue = Source == Source_e.Custom ? Argument : ""
                },
                Location= new LocationData() 
                {
                    Dock = Dock,
                    Size = Size,
                    OffsetX = OffsetX,
                    OffsetY = OffsetY
                }
            };

        public void Fill(QrCodeData srcData, IXObject pict)
        {
            var arg = "";
            var src = srcData.Source;
            var loc = srcData.Location;

            switch (src.Source)
            {
                case Source_e.CustomProperty:
                    arg = src.CustomPropertyName;
                    break;

                case Source_e.PdmWeb2Url:
                    arg = src.PdmWeb2Server;
                    break;

                case Source_e.Custom:
                    arg = src.CustomValue;
                    break;
            }

            Picture = pict;
            Source = src.Source;
            RefDocumentSource = src.ReferencedDocument;
            Argument = arg;

            Dock = loc.Dock;
            Size = loc.Size;
            OffsetX = loc.OffsetX;
            OffsetY = loc.OffsetY;
        }
    }
}
