using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.Bom
{
    public interface IBomValueExtractor
    {
        object GetValue(IBomItemBase item, ValueSource_e src, string arg);
    }
}
