using System;
using Xarial.XCad.Data;
using Xarial.XCad.Geometry;
using Xarial.XToolkit.Services.Expressions;

namespace Xarial.CadPlus.Examples
{
    public class BodyInfo 
    {
        public IXBody Body { get; }
        public string Name { get; }
        public int Quantity { get; }
        public IXPropertyRepository Properties { get; }

        public BodyInfo(IXBody body, string name, int qty, IXPropertyRepository properties) 
        {
            Body = body;
            Name = name;
            Quantity = qty;
            Properties = properties;
        }
    }

    public class ExportBodiesMacroVariableValueProvider : VariableValueProvider<BodyInfo> 
    {
        public const string VAR_BODY_NAME = "bodyName";
        public const string VAR_QTY = "qty";
        public const string VAR_CUT_LIST_PRP = "cutListPrp";
        
        public override object Provide(string name, object[] args, BodyInfo context)
        {
            switch (name)
            {
                case VAR_BODY_NAME:
                    return context.Name;

                case VAR_QTY:
                    return context.Quantity;

                case VAR_CUT_LIST_PRP:
                    if (context.Properties != null)
                    {
                        if (context.Properties.TryGet(args[0]?.ToString(), out var prp))
                        {
                            return prp.Value;
                        }
                    }

                    return "";

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
