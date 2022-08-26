//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Xarial.CadPlus.Plus.Services;
using Xarial.XToolkit.Wpf.Converters;

namespace Xarial.CadPlus.Plus.Shared.Converters
{
    public class IsPreparingBatchJobConverter : BooleanUniversalConverter
    {
        public IsPreparingBatchJobConverter() 
        {
            TrueValue = Visibility.Visible;
            FalseValue = Visibility.Collapsed;
        }

        protected override bool? ConvertValueToBool(object value)
        {
            if (value is BatchJobStatus_e)
            {
                var status = (BatchJobStatus_e)value;

                return status == BatchJobStatus_e.NotStarted || status == BatchJobStatus_e.Initializing;
            }
            else
            {
                return true;
            }
        }
    }
}
