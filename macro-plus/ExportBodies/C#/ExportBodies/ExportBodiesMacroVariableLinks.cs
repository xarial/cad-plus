using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Examples.Properties;
using Xarial.XToolkit.Services.Expressions;
using Xarial.XToolkit.Wpf.Controls;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Examples
{
    public class ExportBodiesMacroVariableLinks : List<IExpressionVariableLink>
    {
        public ExportBodiesMacroVariableLinks() 
        {
            Add(new ExpressionVariableLink("Body Name", "Name of the body or cut-list", Resources.body.ToBitmapImage(true),
                s => new ExpressionTokenVariable(ExportBodiesMacroVariableValueProvider.VAR_BODY_NAME, null), false));

            Add(new ExpressionVariableLink("Body Quantity", "Quantity of bodies within cut-list", Resources.qty.ToBitmapImage(true),
                s => new ExpressionTokenVariable(ExportBodiesMacroVariableValueProvider.VAR_QTY, null), false));

            Add(new ExpressionVariableLink("Cut-List Custom Property", "Value of the custom property of cut-list", Resources.cut_list_prp.ToBitmapImage(true),
                s => new ExpressionTokenVariable(ExportBodiesMacroVariableValueProvider.VAR_CUT_LIST_PRP, new IExpressionToken[] { new ExpressionTokenText("") }), true));
        }
    }
}
