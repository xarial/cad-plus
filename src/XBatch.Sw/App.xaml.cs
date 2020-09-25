using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Xarial.CadPlus.XBatch.Base;

namespace Xarial.CadPlus.XBatch.Sw
{
    public class XBatchSwApp : XBatchApp
    {
        public override IApplicationProvider GetApplicationProvider()
        {
            return null;
        }
    }
}
