//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using Xarial.CadPlus.CustomToolbar;
using Xarial.CadPlus.CustomToolbar.Services;

namespace Xarial.CadPlus.Toolbar.UI.Converters
{
    public class MacroPathToEntryPointsConverter : IMultiValueConverter
    {
        private readonly IMacroEntryPointsExtractor m_Extractor;

        public MacroPathToEntryPointsConverter()
            : this(ToolbarModule.Resolve<IMacroEntryPointsExtractor>())
        {
        }

        public MacroPathToEntryPointsConverter(IMacroEntryPointsExtractor extractor)
        {
            m_Extractor = extractor;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is string && values[1] is string)
            {   
                try
                {
                    var macroPath = (string)values[0];
                    var workDir = (string)values[1];

                    return m_Extractor.GetEntryPoints(macroPath, workDir);
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}