using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.CadPlus.Plus.Shared.Exceptions;
using Xarial.XToolkit.Wpf.Utils;

namespace Xarial.CadPlus.Plus.Shared.Services
{
    public interface IXCadMacroProvider
    {
        IXCadMacro GetMacro(string path);
    }

    public class XCadMacroProvider : IXCadMacroProvider
    {
        public static FileFilter Filter => new FileFilter("xCAD Macro File", "*.dll");

        public IXCadMacro GetMacro(string path)
        {
            var catalog = new AssemblyCatalog(path);

            var container = new CompositionContainer(catalog);

            var macros = container.GetExports<IXCadMacro>();

            if (macros.Any())
            {
                if (macros.Count() == 1)
                {
                    return macros.First().Value;
                }
                else
                {
                    throw new UserException("More than one xCAD macro found in the specified dll");
                }
            }
            else
            {
                throw new NotXCadMacroDllException();
            }
        }
    }
}
