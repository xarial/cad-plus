//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xarial.CadPlus.Batch.Base.Properties;
using Xarial.CadPlus.Plus.Services;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Batch.Base.Core
{
    public class JobItemOperationMacroDefinition : IJobItemOperationDefinition
    {
        private static readonly BitmapImage m_Icon;

        static JobItemOperationMacroDefinition() 
        {
            m_Icon = Resources.macro_icon_default.ToBitmapImage(true);
        }

        public string Name { get; }

        public BitmapImage Icon => m_Icon;

        public MacroData MacroData { get; }

        public JobItemOperationMacroDefinition(MacroData macroData)
        {
            MacroData = macroData;
            Name = Path.GetFileNameWithoutExtension(macroData.FilePath);
        }
    }
}
