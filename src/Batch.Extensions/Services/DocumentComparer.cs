//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using Xarial.XCad.Documents;

namespace Xarial.CadPlus.Batch.Extensions.Services
{
    internal class DocumentComparer : IEqualityComparer<IXDocument>
    {
        public bool Equals(IXDocument x, IXDocument y)
            => string.Equals(x.Path, y.Path, StringComparison.CurrentCultureIgnoreCase);

        public int GetHashCode(IXDocument obj)
            => 0;
    }
}
