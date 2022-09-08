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

namespace Xarial.CadPlus.Drawing.QrCode.Data
{
    public class QrCodeInfoVersionTransformer : BaseUserSettingsVersionsTransformer
    {
        public QrCodeInfoVersionTransformer()
        {
            //TODO: implement converter

            Add(new Version("1.0"), new Version("2.0"), t => 
            {
                //public enum Source_e
                //{
                //    [Title("Custom Property")]
                //    CustomProperty,

                //    [Title("File Path")]
                //    FilePath,

                //    [Title("Part Number")]
                //    PartNumber,

                //    [Title("PDM Vault Link")]
                //    PdmVaultLink,

                //    [Title("PDM Web2 Url")]
                //    PdmWeb2Url,

                //    Custom
                //}

                //public Source_e Source
                //{
                //    get => m_Source;
                //    set
                //    {
                //        m_Source = value;
                //        Changed?.Invoke(this);
                //    }
                //}

                //public bool RefDocumentSource
                //{
                //    get => m_RefDocumentSource;
                //    set
                //    {
                //        m_RefDocumentSource = value;
                //        Changed?.Invoke(this);
                //    }
                //}

                //public string Argument
                //{
                //    get => m_Argument;
                //    set
                //    {
                //        m_Argument = value;
                //        Changed?.Invoke(this);
                //    }
                //}

                return t;
            });
        }
    }

    [UserSettingVersion("2.0", typeof(QrCodeInfoVersionTransformer))]
    public class QrCodeInfo
    {
        public event Action<QrCodeInfo> Changed;

        private IXObject m_Picture;
        private string m_Expression;
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
                Changed?.Invoke(this);
            }
        }

        public string Expression
        {
            get => m_Expression;
            set
            {
                m_Expression = value;
                Changed?.Invoke(this);
            }
        }

        public double Size
        {
            get => m_Size;
            set
            {
                m_Size = value;
                Changed?.Invoke(this);
            }
        }

        public Dock_e Dock
        {
            get => m_Dock;
            set
            {
                m_Dock = value;
                Changed?.Invoke(this);
            }
        }

        public double OffsetX
        {
            get => m_OffsetX;
            set
            {
                m_OffsetX = value;
                Changed?.Invoke(this);
            }
        }

        public double OffsetY
        {
            get => m_OffsetY;
            set
            {
                m_OffsetY = value;
                Changed?.Invoke(this);
            }
        }

        public QrCodeData ToData()
            => new QrCodeData()
            {
                Source = new SourceData()
                {
                    Expression = Expression
                },
                Location = new LocationData()
                {
                    Dock = Dock,
                    Size = Size,
                    OffsetX = OffsetX,
                    OffsetY = OffsetY
                }
            };

        public void Fill(QrCodeData srcData, IXObject pict)
        {
            var src = srcData.Source;
            var loc = srcData.Location;

            Picture = pict;
            Expression = src.Expression;

            Dock = loc.Dock;
            Size = loc.Size;
            OffsetX = loc.OffsetX;
            OffsetY = loc.OffsetY;
        }
    }
}
