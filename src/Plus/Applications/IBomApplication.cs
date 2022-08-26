//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Delegates;
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Plus.Applications
{
    public class BomApplicationCommandManager 
    {
        public static class ViewTab
        {
            public const string Name = "View";

            public const string DataGroupName = "Data";
            public const string DisplayGroupName = "Display";
        }
    }

    public interface IBomApplication : IDocumentConsumerApplication, IHasCommandManager
    {
        void ViewBom(string filePath);
    }
}
