using System;
using Xarial.XCad.Geometry;
using Xarial.XToolkit.Services.Expressions;

namespace Xarial.CadPlus.Examples
{
    public class ExportBodiesMacroVariableValueProvider : VariableValueProvider<IXBody> 
    {
        public const string VAR_BODY_NAME = "bodyName";

        public override object Provide(string name, object[] args, IXBody context)
        {
            switch (name)
            {
                case VAR_BODY_NAME:
                    return context.Name;

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
