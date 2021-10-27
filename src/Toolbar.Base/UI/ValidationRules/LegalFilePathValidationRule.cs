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
    public class LegalFilePathValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                var filePath = value as string;

                if (!string.IsNullOrEmpty(filePath))
                {
                    var fileInfo = new FileInfo(filePath);
                    return ValidationResult.ValidResult;
                }
                else 
                {
                    throw new Exception("File path is empty");
                }
            }
            catch (Exception ex)
            {
                return new ValidationResult(false, ex.Message);
            }
        }
    }
}
