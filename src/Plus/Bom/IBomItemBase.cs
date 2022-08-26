//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Xarial.CadPlus.Plus.Bom
{
    public interface IBomItemBase
    {
        string Name { get; }

        string DisplayName { get; }

        double Quantity { get; }
        IBomItemBase[] Children { get; }
        object GetValue(ValueSource_e src, string arg);

        BitmapSource Preview { get; }

        ItemType_e Type { get; }
    }
}
