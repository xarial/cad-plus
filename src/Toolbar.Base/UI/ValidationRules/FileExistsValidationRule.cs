//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Xarial.CadPlus.Toolbar.UI.ValidationRules
{
    public class FileExistsValidationRule : ValidationRule
    {
        public bool AllowEmptyValues { get; set; } = false;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!string.IsNullOrEmpty(value as string))
            {
                if (!File.Exists(value as string))
                {
                    return new ValidationResult(false, "File does not exist");
                }
                else 
                {
                    return ValidationResult.ValidResult;
                }
            }
            else 
            {
                if (AllowEmptyValues)
                {
                    return ValidationResult.ValidResult;
                }
                else
                {
                    return new ValidationResult(false, "Path is not specified");
                }
            }
        }
    }
}
