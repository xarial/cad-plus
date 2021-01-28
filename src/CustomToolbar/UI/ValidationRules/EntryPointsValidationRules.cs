//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Xarial.CadPlus.CustomToolbar.Structs;

namespace Xarial.CadPlus.CustomToolbar.UI.ValidationRules
{
    public class EntryPointsValidationRules : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is IEnumerable<MacroStartFunction>)
            {
                if (!(value as IEnumerable<MacroStartFunction>).Any())
                {
                    return new ValidationResult(false, "No enry points in the macro");
                }
                else
                {
                    return ValidationResult.ValidResult;
                }
            }
            else 
            {
                return new ValidationResult(false, "Failed to extract entry points");
            }
        }
    }
}
