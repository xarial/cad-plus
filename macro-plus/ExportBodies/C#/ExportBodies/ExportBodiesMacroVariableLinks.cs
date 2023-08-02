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
            Add(new ExpressionVariableLink("Body Name", "Name of th body", Resources.body.ToBitmapImage(true),
                s => new ExpressionTokenVariable(ExportBodiesMacroVariableValueProvider.VAR_BODY_NAME, null), false));
        }
    }
}
