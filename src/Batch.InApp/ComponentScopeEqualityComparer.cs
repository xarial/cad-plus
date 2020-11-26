﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents;

namespace Xarial.CadPlus.Batch.InApp
{
    internal class ComponentScopeEqualityComparer : IEqualityComparer<IXComponent>
    {
        public bool Equals(IXComponent x, IXComponent y)
        {
            //TODO: check by path
            return x == y;
        }

        public int GetHashCode(IXComponent obj)
        {
            return 0;
        }
    }
}
