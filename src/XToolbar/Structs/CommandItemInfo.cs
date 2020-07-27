//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

namespace Xarial.CadPlus.XToolbar.Structs
{
    public abstract class CommandItemInfo
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string IconPath { get; set; }
    }
}