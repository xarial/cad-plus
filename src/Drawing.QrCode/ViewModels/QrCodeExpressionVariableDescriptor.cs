using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xarial.CadPlus.Drawing.QrCode.Data;
using Xarial.CadPlus.Drawing.QrCode.Properties;
using Xarial.CadPlus.Drawing.QrCode.Services;
using Xarial.XToolkit.Services.Expressions;
using Xarial.XToolkit.Wpf.Controls;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Drawing.QrCode.ViewModels
{
    public class QrCodeExpressionVariableDescriptor : IExpressionVariableDescriptor
    {
        private static readonly BitmapImage m_FilePathIcon;
        private static readonly BitmapImage m_CustomPropertyIcon;
        private static readonly BitmapImage m_ConfigurationIcon;
        private static readonly BitmapImage m_PartNumberIcon;
        private static readonly BitmapImage m_PdmVaultLinkIcon;
        private static readonly BitmapImage m_Web2UrlIcon;

        static QrCodeExpressionVariableDescriptor() 
        {
            m_FilePathIcon = Resources.file_path.ToBitmapImage(true);
            m_CustomPropertyIcon = Resources.custom_property.ToBitmapImage(true);
            m_ConfigurationIcon = Resources.configuration.ToBitmapImage(true);
            m_PartNumberIcon = Resources.part_number.ToBitmapImage(true);
            m_PdmVaultLinkIcon = Resources.pdm_link.ToBitmapImage(true);
            m_Web2UrlIcon = Resources.web2_url.ToBitmapImage(true);
        }

        public ExpressionVariableArgumentDescriptor[] GetArguments(IExpressionTokenVariable variable, out bool dynamic)
        {
            dynamic = false;

            switch (variable.Name)
            {
                case QrCodeDataSourceExpressionSolver.VAR_FILE_PATH:
                    return new ExpressionVariableArgumentDescriptor[] 
                    {
                        ExpressionVariableArgumentDescriptor.CreateOptions<FilePathSource_e>("Source", "Source of file path", null),
                        ExpressionVariableArgumentDescriptor.CreateToggle("Referenced Document", "Get file path of referenced document", null)
                    };
                case QrCodeDataSourceExpressionSolver.VAR_CUSTOM_PRP:
                    return new ExpressionVariableArgumentDescriptor[]
                    {
                        ExpressionVariableArgumentDescriptor.CreateText("Property Name", "Name of the custom property", null),
                        ExpressionVariableArgumentDescriptor.CreateToggle("Referenced Document", "Get custom property of referenced document", null)
                    };
                case QrCodeDataSourceExpressionSolver.VAR_CONF_NAME:
                    return null;
                case QrCodeDataSourceExpressionSolver.VAR_PART_NUMBER:
                    return null;
                case QrCodeDataSourceExpressionSolver.VAR_PDM_VAULT_LINK:
                    return new ExpressionVariableArgumentDescriptor[]
                    {
                        ExpressionVariableArgumentDescriptor.CreateOptions<PdmVaultLinkAction_e>("Link Action", "Action to perform on the link click", null),
                        ExpressionVariableArgumentDescriptor.CreateToggle("Referenced Document", "Get web2 url of referenced document", null)
                    };
                case QrCodeDataSourceExpressionSolver.VAR_PDM_WEB2_URL:
                    return new ExpressionVariableArgumentDescriptor[]
                    {
                        ExpressionVariableArgumentDescriptor.CreateText("Server Domain", "Domain url of the web-service", null),
                        ExpressionVariableArgumentDescriptor.CreateToggle("Referenced Document", "Get web2 url of referenced document", null)
                    };
                default:
                    throw new NotSupportedException();
            }
        }

        public Brush GetBackground(IExpressionTokenVariable variable)
        {
            switch (variable.Name)
            {
                case QrCodeDataSourceExpressionSolver.VAR_FILE_PATH:
                    return Brushes.LightGray;
                case QrCodeDataSourceExpressionSolver.VAR_CUSTOM_PRP:
                    return Brushes.LightBlue;
                case QrCodeDataSourceExpressionSolver.VAR_CONF_NAME:
                    return Brushes.LightSalmon;
                case QrCodeDataSourceExpressionSolver.VAR_PART_NUMBER:
                    return Brushes.LightCoral;
                case QrCodeDataSourceExpressionSolver.VAR_PDM_VAULT_LINK:
                    return Brushes.LightGreen;
                case QrCodeDataSourceExpressionSolver.VAR_PDM_WEB2_URL:
                    return Brushes.LightYellow;
                default:
                    throw new NotSupportedException();
            }
        }

        public string GetDescription(IExpressionTokenVariable variable)
        {
            switch (variable.Name)
            {
                case QrCodeDataSourceExpressionSolver.VAR_FILE_PATH:
                    return "File path";
                case QrCodeDataSourceExpressionSolver.VAR_CUSTOM_PRP:
                    return "Custom property";
                case QrCodeDataSourceExpressionSolver.VAR_CONF_NAME:
                    return "Configuration name";
                case QrCodeDataSourceExpressionSolver.VAR_PART_NUMBER:
                    return "Part number";
                case QrCodeDataSourceExpressionSolver.VAR_PDM_VAULT_LINK:
                    return "PDM conisio url link";
                case QrCodeDataSourceExpressionSolver.VAR_PDM_WEB2_URL:
                    return "PDM Web2 url";
                default:
                    throw new NotSupportedException();
            }
        }

        public ImageSource GetIcon(IExpressionTokenVariable variable)
        {
            switch (variable.Name)
            {
                case QrCodeDataSourceExpressionSolver.VAR_FILE_PATH:
                    return m_FilePathIcon;
                case QrCodeDataSourceExpressionSolver.VAR_CUSTOM_PRP:
                    return m_CustomPropertyIcon;
                case QrCodeDataSourceExpressionSolver.VAR_CONF_NAME:
                    return m_ConfigurationIcon;
                case QrCodeDataSourceExpressionSolver.VAR_PART_NUMBER:
                    return m_PartNumberIcon;
                case QrCodeDataSourceExpressionSolver.VAR_PDM_VAULT_LINK:
                    return m_PdmVaultLinkIcon;
                case QrCodeDataSourceExpressionSolver.VAR_PDM_WEB2_URL:
                    return m_Web2UrlIcon;
                default:
                    throw new NotSupportedException();
            }
        }

        public string GetTitle(IExpressionTokenVariable variable)
        {
            switch (variable.Name) 
            {
                case QrCodeDataSourceExpressionSolver.VAR_FILE_PATH:
                    if (Enum.TryParse<FilePathSource_e>((variable.Arguments?.FirstOrDefault() as IExpressionTokenText)?.Text, true, out var filePathSrc))
                    {
                        switch (filePathSrc) 
                        {
                            case FilePathSource_e.FullPath:
                                return "File Path";

                            case FilePathSource_e.Title:
                                return "File Title";

                            default:
                                return "???";
                        }
                    }
                    else 
                    {
                        return "???";
                    }
                case QrCodeDataSourceExpressionSolver.VAR_CUSTOM_PRP:
                    return (variable.Arguments?.FirstOrDefault() as IExpressionTokenText)?.Text ?? "???";
                case QrCodeDataSourceExpressionSolver.VAR_CONF_NAME:
                    return "Configuration";
                case QrCodeDataSourceExpressionSolver.VAR_PART_NUMBER:
                    return "Part Number";
                case QrCodeDataSourceExpressionSolver.VAR_PDM_VAULT_LINK:
                    return "PDM Link";
                case QrCodeDataSourceExpressionSolver.VAR_PDM_WEB2_URL:
                    return "PDM Web2 Url";
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
