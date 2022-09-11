using EPDM.Interop.epdm;
using System;
using System.IO;
using System.Linq;
using Xarial.CadPlus.Drawing.QrCode.Data;
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.CadPlus.Plus.Services;
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XToolkit.Services.Expressions;

namespace Xarial.CadPlus.Drawing.QrCode.Services
{
    public class DataSourceDocument : IDisposable
    {
        public IXApplication Application { get; }
        public IXDocument ReferencedDocument { get; }
        public IXConfiguration ReferencedConfiguration { get; }
        public IXDrawing Drawing { get; }

        private readonly IXLogger m_Logger;

        private readonly IDocumentMetadataAccessLayer m_DrwMal;
        private readonly IDocumentMetadataAccessLayer m_RefDocMal;

        public DataSourceDocument(IDocumentMetadataAccessLayerProvider docMalProvider, IXApplication app, IXDrawing drw, IXLogger logger)
        {
            m_Logger = logger;

            m_DrwMal = docMalProvider.Create(drw, true);

            Application = app;

            m_Logger.Log($"Data source drawing is fallback: {m_DrwMal.IsFallbackDocument}", LoggerMessageSeverity_e.Debug);

            Drawing = (IXDrawing)m_DrwMal.Document;

            var view = Drawing.Sheets.Active.DrawingViews.FirstOrDefault();

            if (view != null)
            {
                var refDoc = view.ReferencedDocument;

                if (refDoc == null)
                {
                    throw new UserException("View does not have a document");
                }

                var conf = view.ReferencedConfiguration;

                m_RefDocMal = docMalProvider.Create(refDoc, true);

                m_Logger.Log($"Data source reference document is fallback: {m_RefDocMal.IsFallbackDocument}", LoggerMessageSeverity_e.Debug);

                refDoc = (IXDocument3D)m_RefDocMal.Document;

                if (m_RefDocMal.IsFallbackDocument)
                {
                    conf = refDoc.Configurations[conf.Name];
                }

                ReferencedDocument = refDoc;
                ReferencedConfiguration = conf;
            }
        }

        public void Dispose()
        {
            m_DrwMal?.Dispose();
            m_RefDocMal?.Dispose();
        }
    }

    public class QrCodeDataSourceExpressionSolver : ExpressionSolver<DataSourceDocument>
    {
        public const string VAR_FILE_PATH = "filepath";
        public const string VAR_CUSTOM_PRP = "prp";
        public const string VAR_CONF_NAME = "conf";
        public const string VAR_PART_NUMBER = "partnmb";
        public const string VAR_PDM_VAULT_LINK = "pdmlink";
        public const string VAR_PDM_WEB2_URL = "pdm2web2url";

        public QrCodeDataSourceExpressionSolver() : base(SolveVariable, StringComparison.CurrentCultureIgnoreCase)
        {
        }

        private static string SolveVariable(string name, string[] args, DataSourceDocument context)
        {
            switch (name)
            {
                case VAR_FILE_PATH:
                    return GetFilePath(args, context);

                case VAR_CUSTOM_PRP:
                    return GetCustomProperty(args, context);

                case VAR_CONF_NAME:
                    return context.ReferencedConfiguration.Name;

                case VAR_PART_NUMBER:
                    return context.ReferencedConfiguration.PartNumber;

                case VAR_PDM_VAULT_LINK:
                    return GetPdmVaultLink(args, context);

                case VAR_PDM_WEB2_URL:
                    return GetPdmWeb2Url(args, context);

                default:
                    throw new UserException($"Failed to evaluate expression: variable '{name}' is not recognized");
            }
        }

        private static string GetFilePath(string[] args, DataSourceDocument context)
        {
            if (args.Length != 2)
            {
                throw new UserException($"'{VAR_FILE_PATH}' variable requires 2 arguments");
            }

            if (!Enum.TryParse<FilePathSource_e>(args[0], true, out var filePathSrc))
            {
                throw new UserException("Failed to parse the file path source");
            }

            if (!bool.TryParse(args[1], out var refDoc))
            {
                throw new UserException("Failed to parse the reference document");
            }

            var targDoc = refDoc ? context.ReferencedDocument : context.Drawing;

            if (targDoc != null)
            {
                switch (filePathSrc)
                {
                    case FilePathSource_e.FullPath:
                        return targDoc.Path;

                    case FilePathSource_e.Title:
                        return targDoc.Title;

                    default:
                        throw new NotSupportedException();
                }
            }
            else 
            {
                throw new UserException("No referenced document found in the drawing sheet");
            }
        }

        private static string GetCustomProperty(string[] args, DataSourceDocument context)
        {
            if (args.Length != 2)
            {
                throw new UserException($"'{VAR_CUSTOM_PRP}' variable requires 2 arguments");
            }

            var prpName = args[0];

            if (!bool.TryParse(args[1], out var refDoc))
            {
                throw new UserException("Failed to parse the reference document");
            }

            var targDoc = refDoc ? context.ReferencedDocument : context.Drawing;

            if (targDoc != null)
            {
                IXProperty prp;

                if (refDoc)
                {
                    if (context.ReferencedConfiguration.Properties.TryGet(prpName, out prp))
                    {
                        return prp.Value?.ToString();
                    }
                }

                if (targDoc.Properties.TryGet(prpName, out prp))
                {
                    return prp.Value?.ToString();
                }

                throw new UserException($"Custom property '{prpName}' is not found");
            }
            else 
            {
                throw new UserException("No referenced document found in the drawing sheet");
            }
        }

        private static string GetPdmVaultLink(string[] args, DataSourceDocument context)
        {
            if (args.Length != 2)
            {
                throw new UserException($"'{VAR_PDM_VAULT_LINK}' variable requires 2 arguments");
            }

            if (!Enum.TryParse<PdmVaultLinkAction_e>(args[0], true, out var actionType))
            {
                throw new UserException("Failed to parse the action");
            }

            if (!bool.TryParse(args[1], out var refDoc))
            {
                throw new UserException("Failed to parse the reference document");
            }

            string action;

            switch (actionType)
            {
                case PdmVaultLinkAction_e.Open:
                    action = "open";
                    break;
                case PdmVaultLinkAction_e.View:
                    action = "view";
                    break;
                case PdmVaultLinkAction_e.Explore:
                    action = "explore";
                    break;
                case PdmVaultLinkAction_e.Get:
                    action = "get";
                    break;
                case PdmVaultLinkAction_e.Lock:
                    action = "lock";
                    break;
                case PdmVaultLinkAction_e.Properties:
                    action = "properties";
                    break;
                case PdmVaultLinkAction_e.History:
                    action = "history";
                    break;
                default:
                    throw new NotSupportedException();
            }

            var targDoc = refDoc ? context.ReferencedDocument : context.Drawing;

            if (targDoc != null)
            {
                var filePath = FindRelativeVaultPath(targDoc.Path, context.Application, out var vault);

                var file = vault.GetFileFromPath(filePath, out var folder);

                if (file == null)
                {
                    throw new UserException("File is not in the vault");
                }

                return $"conisio://{vault.Name}/{action}?projectid={folder.ID}&documentid={file.ID}&objecttype={(int)file.ObjectType}";
            }
            else 
            {
                throw new UserException("No referenced document found in the drawing sheet");
            }
        }

        private static string GetPdmWeb2Url(string[] args, DataSourceDocument context)
        {
            if (args.Length != 2)
            {
                throw new UserException($"'{VAR_CUSTOM_PRP}' variable requires 2 arguments");
            }

            var server = args[0];

            if (!bool.TryParse(args[1], out var refDoc))
            {
                throw new UserException("Failed to parse the reference document");
            }

            var targDoc = refDoc ? context.ReferencedDocument : context.Drawing;

            if (targDoc != null)
            {
                if (string.IsNullOrEmpty(server))
                {
                    throw new UserException("Url of Web2 server is not specified");
                }

                var vaultRelPath = FindRelativeVaultPath(targDoc.Path, context.Application, out var vault);

                return $"{server}/{vault.Name}/{Path.GetDirectoryName(vaultRelPath).Replace('\\', '/')}?view=bom&file={Path.GetFileName(vaultRelPath)}";
            }
            else 
            {
                throw new UserException("No referenced document found in the drawing sheet");
            }
        }

        private static string FindRelativeVaultPath(string filePath, IXApplication app, out IEdmVault5 vault)
        {
            vault = new EdmVault5Class();

            var vaultName = vault.GetVaultNameFromPath(filePath);

            if (string.IsNullOrEmpty(vaultName))
            {
                throw new UserException("This file is not a part of any vault");
            }

            vault.LoginAuto(vaultName, app.WindowHandle.ToInt32());

            if (vault.IsLoggedIn)
            {
                var rootFolderPath = vault.RootFolderPath;

                return filePath.Substring(rootFolderPath.Length + 1, filePath.Length - rootFolderPath.Length - 1);
            }
            else
            {
                throw new UserException($"Failed to login to '{vaultName}' vault");
            }
        }
    }
}