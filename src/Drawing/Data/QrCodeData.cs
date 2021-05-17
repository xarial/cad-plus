//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
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
    public class QrCodeDataVersionTransformer : BaseUserSettingsVersionsTransformer
    {
        public QrCodeDataVersionTransformer()
        {
        }
    }

    [UserSettingVersion("1.0", typeof(QrCodeDataVersionTransformer))]
    public class QrCodeData
    {
        public event Action<QrCodeData> Changed;

        private IXObject m_Picture;
        private Source_e m_Source;
        private string m_Argument;
        private bool m_RefDocumentSource;

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

        public SourceData ToSourceData()
            => new SourceData()
            {
                Source = Source,
                ReferencedDocument = RefDocumentSource,
                CustomPropertyName = Source == Source_e.CustomProperty ? Argument : "",
                PdmWeb2Server = Source == Source_e.PdmWeb2Url ? Argument : "",
                CustomValue = Source == Source_e.Custom ? Argument : ""
            };

        public void Fill(SourceData srcData, IXObject pict)
        {
            var arg = "";
            switch (srcData.Source)
            {
                case Source_e.CustomProperty:
                    arg = srcData.CustomPropertyName;
                    break;

                case Source_e.PdmWeb2Url:
                    arg = srcData.PdmWeb2Server;
                    break;

                case Source_e.Custom:
                    arg = srcData.CustomValue;
                    break;
            }

            Picture = pict;
            Source = srcData.Source;
            RefDocumentSource = srcData.ReferencedDocument;
            Argument = arg;
        }
    }
}
