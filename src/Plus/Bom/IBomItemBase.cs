using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.Bom
{
    public interface IBomItemBase
    {
        string Name { get; }

        double Quantity { get; }
        IBomItemBase[] Children { get; }
        object GetValue(ValueSource_e src, string arg);

        Image Preview { get; }

        ItemType_e Type { get; }
    }
}
