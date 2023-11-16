using System;
using Xarial.CadPlus.Examples.Properties;
using Xarial.XToolkit.Services.Expressions;
using Xarial.XToolkit.Wpf.Controls;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Examples
{
    public class ExportBodiesMacroVariablesDescriptor : IExpressionVariableDescriptor
    {   
        public ExpressionVariableArgumentDescriptor[] GetArguments(IExpressionTokenVariable variable, out bool dynamic)
        {
            switch (variable.Name) 
            {
                case ExportBodiesMacroVariableValueProvider.VAR_BODY_NAME:
                    dynamic = false;
                    return null;

                case ExportBodiesMacroVariableValueProvider.VAR_QTY:
                    dynamic = false;
                    return null;

                case ExportBodiesMacroVariableValueProvider.VAR_CUT_LIST_PRP:
                    dynamic = false;
                    return new ExpressionVariableArgumentDescriptor[]
                    {
                        ExpressionVariableArgumentDescriptor.CreateText("Property Name", "Name of the cut-list custom property", null) 
                    };

                default:
                    throw new NotSupportedException();
            }
        }

        public System.Windows.Media.Brush GetBackground(IExpressionTokenVariable variable)
        {
            switch (variable.Name)
            {
                case ExportBodiesMacroVariableValueProvider.VAR_BODY_NAME:
                    return System.Windows.Media.Brushes.LightSlateGray;

                case ExportBodiesMacroVariableValueProvider.VAR_QTY:
                    return System.Windows.Media.Brushes.LightSeaGreen;

                case ExportBodiesMacroVariableValueProvider.VAR_CUT_LIST_PRP:
                    return System.Windows.Media.Brushes.Linen;

                default:
                    throw new NotSupportedException();
            }
        }

        public string GetDescription(IExpressionTokenVariable variable)
        {
            switch (variable.Name)
            {
                case ExportBodiesMacroVariableValueProvider.VAR_BODY_NAME:
                    return "Name of the body";

                case ExportBodiesMacroVariableValueProvider.VAR_QTY:
                    return "Quantity of bodies";

                case ExportBodiesMacroVariableValueProvider.VAR_CUT_LIST_PRP:
                    return "Custom property value of cut-list item";

                default:
                    throw new NotSupportedException();
            }
        }

        public System.Windows.Media.ImageSource GetIcon(IExpressionTokenVariable variable)
        {
            switch (variable.Name)
            {
                case ExportBodiesMacroVariableValueProvider.VAR_BODY_NAME:
                    return Resources.body.ToBitmapImage(true);

                case ExportBodiesMacroVariableValueProvider.VAR_QTY:
                    return Resources.qty.ToBitmapImage(true);

                case ExportBodiesMacroVariableValueProvider.VAR_CUT_LIST_PRP:
                    return Resources.cut_list_prp.ToBitmapImage(true);

                default:
                    throw new NotSupportedException();
            }
        }

        public string GetTitle(IExpressionTokenVariable variable)
        {
            switch (variable.Name)
            {
                case ExportBodiesMacroVariableValueProvider.VAR_BODY_NAME:
                    return "Body";

                case ExportBodiesMacroVariableValueProvider.VAR_QTY:
                    return "Quantity";

                case ExportBodiesMacroVariableValueProvider.VAR_CUT_LIST_PRP:
                    return "Cut-List Custom Property";

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
