//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using EPDM.Interop.epdm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Drawing.Data;
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.XCad;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.CadPlus.Drawing.Services
{
    public class QrDataProvider
    {
        private readonly IXApplication m_App;

        public QrDataProvider(IXApplication app) 
        {
            m_App = app;
        }

        public string GetData(IXDrawing drw, SourceData srcData) 
        {
            Source_e src = srcData.Source;

            IXDocument doc = drw;

            IXConfiguration conf = null;

            if (srcData.ReferencedDocument) 
            {
                var view = drw.Sheets.Active.DrawingViews.FirstOrDefault();

                if (view == null) 
                {
                    throw new UserException("No drawing views in this document");
                }

                var drwView = (view as ISwDrawingView).DrawingView;
                
                if (drwView.ReferencedDocument == null) 
                {
                    throw new UserException("View does not have a loaded document");
                }

                doc = (m_App as ISwApplication).Documents[drwView.ReferencedDocument];

                conf = ((IXDocument3D)doc).Configurations[drwView.ReferencedConfiguration];
            }

            IEdmVault5 vault;

            switch (src)
            {
                case Source_e.FilePath:
                    return doc.Path;

                case Source_e.PartNumber:
                    if (conf == null)
                    {
                        throw new UserException("Part number can only be extracted from the configuration of part or assembly");
                    }
                    return conf.PartNumber;

                case Source_e.CustomProperty:
                    IXProperty prp = null;

                    if (conf != null)
                    {
                        conf.Properties.TryGet(srcData.CustomPropertyName, out prp);
                    }

                    if (prp == null)
                    {
                        doc.Properties.TryGet(srcData.CustomPropertyName, out prp);
                    }

                    if (prp != null)
                    {
                        return prp.Value?.ToString();
                    }
                    else
                    {
                        throw new UserException("Specified custom property does not exist");
                    }

                case Source_e.PdmVaultLink:
                    const string CONISIO_URL_ACTION = "explore";
                    FindRelativeVaultPath(doc.Path, out vault);
                    var file = vault.GetFileFromPath(doc.Path, out IEdmFolder5 folder);
                    return $"conisio://{vault.Name}/{CONISIO_URL_ACTION}?projectid={folder.ID}&documentid={file.ID}&objecttype={(int)file.ObjectType}";

                case Source_e.PdmWeb2Url:
                    if (string.IsNullOrEmpty(srcData.PdmWeb2Server))
                    {
                        throw new UserException("Url of Web2 server is not specified");
                    }

                    var vaultRelPath = FindRelativeVaultPath(doc.Path, out vault);
                    return $"{srcData.PdmWeb2Server}/{vault.Name}/{Path.GetDirectoryName(vaultRelPath).Replace('\\', '/')}?view=bom&file={Path.GetFileName(vaultRelPath)}";

                case Source_e.Custom:
                    return srcData.CustomValue;

                default:
                    throw new NotSupportedException();
            }
        }

        private string FindRelativeVaultPath(string filePath, out IEdmVault5 vault) 
        {
            vault = new EdmVault5Class();
            
            var vaultName = vault.GetVaultNameFromPath(filePath);

            if (string.IsNullOrEmpty(vaultName)) 
            {
                throw new UserException("This file is not a part of any vault");
            }

            vault.LoginAuto(vaultName, m_App.WindowHandle.ToInt32());

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
