using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.CustomToolbar.Exceptions
{
    public class CustomResolverCodeCompileFailedException : Exception
    {
        private static string GetErrors(CompilerErrorCollection errors) 
        {
            var result = new StringBuilder();
            
            foreach (CompilerError error in errors) 
            {
                result.AppendLine($"{error.ErrorNumber} - {error.ErrorText}");
            }

            return result.ToString();
        }

        public CustomResolverCodeCompileFailedException(CompilerErrorCollection errors) : base(GetErrors(errors))
        {
        }
    }
}
