//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.Atributes
{
    public enum ModuleRelativeOrder_e 
    {
        Before,
        After
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
    public class ModuleOrderAttribute : Attribute
    {
        public int? Order { get; }
        public ModuleRelativeOrder_e? RelativeOrder { get; }
        public Type RelativeToModuleType { get; }

        public ModuleOrderAttribute(int order) 
        {
            Order = order;
        }

        public ModuleOrderAttribute(Type relToModuleType, ModuleRelativeOrder_e relOrder) 
        {
            RelativeToModuleType = relToModuleType;
            RelativeOrder = relOrder;
        }
    }
}
