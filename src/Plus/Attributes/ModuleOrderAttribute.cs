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

    public class ModuleOrderAttribute : Attribute
    {
        public int? Order { get; }
        public ModuleRelativeOrder_e? RelativeOrder { get; }
        public string RelativeToModuleId { get; }

        public ModuleOrderAttribute(int order) 
        {
            Order = order;
        }

        public ModuleOrderAttribute(string relToModuleId, ModuleRelativeOrder_e relOrder) 
        {
            RelativeToModuleId = relToModuleId;
            RelativeOrder = relOrder;
        }
    }
}
