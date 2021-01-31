using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.Exceptions
{
    public class ModuleOrderCircularDependencyException : Exception
    {
        public ModuleOrderCircularDependencyException(IEnumerable<IModule> modules)
            : base($"Circular dependencies detected while ordering modules: {string.Join(", ", modules.Select(m => m.Id))}")
        {
        }
    }
}
