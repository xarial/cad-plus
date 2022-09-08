using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Drawing.QrCode.Data;
using Xarial.CadPlus.Drawing.QrCode.Services;
using Xarial.XToolkit.Services.Expressions;
using Xarial.XToolkit.Wpf.Controls;

namespace Xarial.CadPlus.Drawing.QrCode.ViewModels
{
    public class QrCodeExpressionVariableLinkCollection : ObservableCollection<IExpressionVariableLink>
    {
        public QrCodeExpressionVariableLinkCollection()
        {
            Add(new ExpressionVariableLink("File Path", "Inserts the file path variable", null, 
                s => new ExpressionTokenVariable(QrCodeDataSourceExpressionSolver.VAR_FILE_PATH, 
                new IExpressionToken[] 
                {
                    new ExpressionTokenText(FilePathSource_e.FullPath.ToString()),
                    new ExpressionTokenText(false.ToString()) 
                }), false));
            
            Add(new ExpressionVariableLink("Custom Property", "Inserts custom property value variable", null,
                s => new ExpressionTokenVariable(QrCodeDataSourceExpressionSolver.VAR_CUSTOM_PRP,
                new IExpressionToken[]
                {
                    new ExpressionTokenText("Description"),
                    new ExpressionTokenText(false.ToString())
                }), true));

            Add(new ExpressionVariableLink("Configuration", "Inserts the referenced configuration name variable", null,
                s => new ExpressionTokenVariable(QrCodeDataSourceExpressionSolver.VAR_CONF_NAME, null), false));

            Add(new ExpressionVariableLink("Part Number", "Inserts the part number of the referenced configuration", null,
                s => new ExpressionTokenVariable(QrCodeDataSourceExpressionSolver.VAR_PART_NUMBER, null), false));

            Add(new ExpressionVariableLink("PDM Vault Link", "Inserts the conision url hyperlink variable", null,
                s => new ExpressionTokenVariable(QrCodeDataSourceExpressionSolver.VAR_PDM_VAULT_LINK,
                new IExpressionToken[]
                {
                    new ExpressionTokenText(PdmVaultLinkAction_e.Explore.ToString()),
                    new ExpressionTokenText(false.ToString())
                }), false));

            Add(new ExpressionVariableLink("PDM Web2 Url", "Inserts the PDM Web2 link variable", null,
                s => new ExpressionTokenVariable(QrCodeDataSourceExpressionSolver.VAR_PDM_WEB2_URL,
                new IExpressionToken[]
                {
                    new ExpressionTokenText("http://localhost"),
                    new ExpressionTokenText(false.ToString())
                }), true));
        }
    }
}
