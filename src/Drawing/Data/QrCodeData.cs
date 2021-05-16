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
        private Dock_e m_Dock;
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

        public Dock_e Dock
        {
            get => m_Dock;
            set
            {
                m_Dock = value;
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
    }
}
