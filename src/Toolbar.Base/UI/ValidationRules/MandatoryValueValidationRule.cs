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
using System.Windows.Controls;

namespace Xarial.CadPlus.Toolbar.UI.ValidationRules
{
    public class MandatoryValueValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null || (value is string && string.IsNullOrEmpty((string)value)))
            {
                return new ValidationResult(false, "This field is mandatory");
            }
            else
            {
                return ValidationResult.ValidResult;
            }
        }
    }
}
