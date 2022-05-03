using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad;

namespace Xarial.CadPlus.Plus.Shared.Services
{
    public class XObjectEqualityComparer : XObjectEqualityComparer<IXObject>
    {
    }

    public class XObjectEqualityComparer<T> : IEqualityComparer<T>
        where T : IXObject
    {
        public bool Equals(T x, T y)
            => x.Equals(y);

        public int GetHashCode(T obj)
            => 0;
    }
}
