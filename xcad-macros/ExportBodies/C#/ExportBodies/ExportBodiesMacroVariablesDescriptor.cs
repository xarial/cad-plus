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

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
