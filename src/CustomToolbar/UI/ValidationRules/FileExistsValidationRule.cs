using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Xarial.CadPlus.CustomToolbar.UI.ValidationRules
{
    public class FileExistsValidationRule : ValidationRule
    {
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
                return new ValidationResult(false, "Path is not specified");
            }
        }
    }
}
