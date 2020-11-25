using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.UI.PropertyPage;

namespace Xarial.CadPlus.AddIn.Base
{
    public interface ICustomHandler
    {
        IXPropertyPage<TData> CreatePage<TData>();
    }
}
