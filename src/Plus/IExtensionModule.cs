﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus
{
    public interface IExtensionModule : IDisposable
    {
        void Init(IHostExtensionApplication host);
    }
}
