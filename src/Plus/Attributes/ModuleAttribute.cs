using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.Attributes
{
    public interface IModuleMetadata
    {
        Guid[] TargetHostIds { get; }
    }

    [AttributeUsage(AttributeTargets.Class), MetadataAttribute]
    public class ModuleAttribute : ExportAttribute, IModuleMetadata
    {
        public Guid[] TargetHostIds { get; }

        public ModuleAttribute(params string[] targetHostIds) : base(typeof(IModule)) 
        {
            TargetHostIds = targetHostIds.Select(g => Guid.Parse(g)).ToArray();
        }
    }
}
