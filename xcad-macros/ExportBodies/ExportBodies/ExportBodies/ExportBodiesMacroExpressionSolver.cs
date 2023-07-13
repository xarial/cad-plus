using System;
using Xarial.XCad.Geometry;
using Xarial.XToolkit.Services.Expressions;

namespace Xarial.CadPlus.Examples
{
    public class ExportBodiesMacroExpressionSolver : ExpressionSolver<IXBody> 
    {
        public const string VAR_BODY_NAME = "bodyName";

        protected override object SolveVariable(string name, object[] args, IXBody context)
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
