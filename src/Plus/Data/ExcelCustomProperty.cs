//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.CadPlus.Plus.Data
{
    [Serializable]
    public class ExcelCustomProperty 
    {
        public string Name { get; }
        public object Value { get; }

        public ExcelCustomProperty(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }
}
