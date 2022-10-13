//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Drawing.QrCode.Services;
using Xarial.CadPlus.Plus.Modules;
using Xarial.CadPlus.Plus.Modules.QrCode;
using Xarial.XCad;
using Xarial.XCad.Features;
using Xarial.XCad.Sketch;
using Xarial.XToolkit.Services.Expressions;
using Xarial.XToolkit.Services.UserSettings;
using Xarial.XToolkit.Services.UserSettings.Attributes;

namespace Xarial.CadPlus.Drawing.QrCode.Data
{
    public class QrCodeInfoVersionTransformer : IVersionsTransformer
    {
        public IReadOnlyList<VersionTransform> Transforms { get; }

        private IExpressionParser m_Parser;

        public QrCodeInfoVersionTransformer()
        {
            Transforms = new VersionTransform[]
            {
                new VersionTransform(new Version(), new Version("2.0"), NoneToVersion2)
            };
        }

        internal void SetExpressonParser(IExpressionParser parser) 
        {
            m_Parser = parser;
        }

        private JToken NoneToVersion2(JToken arr) 
        {
            foreach (var t in (JArray)arr)
            {
                var srcToken = t.Children<JProperty>().First(p => p.Name == "Source");
                var refDocToken = t.Children<JProperty>().First(p => p.Name == "RefDocumentSource");
                var argToken = t.Children<JProperty>().First(p => p.Name == "Argument");

                var src = srcToken.Value.Value<int>();
                var refDoc = refDocToken.Value.Value<bool>();
                var arg = argToken.Value.Value<string>();

                IExpressionToken expr;

                switch (src)
                {
                    case 0://CustomProperty
                        expr = new ExpressionTokenVariable(QrCodeDataSourceExpressionSolver.VAR_CUSTOM_PRP, new IExpressionToken[]
                        {
                            new ExpressionTokenText(arg),
                            new ExpressionTokenText(refDoc.ToString())
                        });
                        break;

                    case 1://FilePath
                        expr = new ExpressionTokenVariable(QrCodeDataSourceExpressionSolver.VAR_FILE_PATH, null);
                        break;

                    case 2://PartNumber
                        expr = new ExpressionTokenVariable(QrCodeDataSourceExpressionSolver.VAR_PART_NUMBER, null);
                        break;

                    case 3://PdmVaultLink
                        expr = new ExpressionTokenVariable(QrCodeDataSourceExpressionSolver.VAR_PDM_VAULT_LINK, new IExpressionToken[]
                        {
                            new ExpressionTokenText(PdmVaultLinkAction_e.Explore.ToString()),
                            new ExpressionTokenText(refDoc.ToString())
                        });
                        break;

                    case 4://PdmWeb2Url
                        expr = new ExpressionTokenVariable(QrCodeDataSourceExpressionSolver.VAR_PDM_WEB2_URL, new IExpressionToken[]
                        {
                            new ExpressionTokenText(arg),
                            new ExpressionTokenText(refDoc.ToString())
                        });
                        break;

                    case 5://Custom
                        expr = new ExpressionTokenText(arg);
                        break;

                    default:
                        throw new NotSupportedException();
                }

                srcToken.Remove();
                refDocToken.Remove();

                argToken.Replace(new JProperty("Expression", m_Parser.CreateExpression(expr)));
            }

            return arr;
        }
    }

    public class QrCodeInfo
    {
        public event Action<QrCodeInfo> Changed;

        private IXSketchPicture m_Picture;
        private string m_Expression;
        private double m_Size;
        private QrCodeDock_e m_Dock;
        private double m_OffsetX;
        private double m_OffsetY;

        public IXSketchPicture Picture
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

        public QrCodeDock_e Dock
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
    }

    [UserSettingVersion("2.0", typeof(QrCodeInfoVersionTransformer))]
    public class QrCodeInfoCollection : ObservableCollection<QrCodeInfo> 
    {
    }
}
