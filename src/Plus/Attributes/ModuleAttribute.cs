//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

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
        string[] TargetHostIds { get; }
    }

    [AttributeUsage(AttributeTargets.Class), MetadataAttribute]
    public class ModuleAttribute : ExportAttribute, IModuleMetadata
    {
        public string[] TargetHostIds { get; }

        public ModuleAttribute(params string[] targetHostIds) : base(typeof(IModule)) 
        {
            TargetHostIds = targetHostIds;
        }
    }
}
