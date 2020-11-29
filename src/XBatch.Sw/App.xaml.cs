//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using Autofac;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Xarial.CadPlus.Common.Sw;
using Xarial.CadPlus.XBatch.Base;

namespace Xarial.CadPlus.XBatch.Sw
{
    public class XBatchSwApp : XBatchApp
    {
        protected override void OnConfigureServices(ContainerBuilder builder) 
        {
            builder.RegisterType<SwApplicationProvider>().As<IApplicationProvider>();
            builder.UsingCommonSwServices();
        }
    }
}
