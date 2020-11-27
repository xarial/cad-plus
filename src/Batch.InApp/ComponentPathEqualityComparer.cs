using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents;

namespace Xarial.CadPlus.Batch.InApp
{
    internal class ComponentPathEqualityComparer : IEqualityComparer<IXComponent>
    {
        public bool Equals(IXComponent x, IXComponent y)
            => string.Equals(x.Path, y.Path, StringComparison.CurrentCultureIgnoreCase);

        public int GetHashCode(IXComponent obj)
        {
            return 0;
        }
    }
}
