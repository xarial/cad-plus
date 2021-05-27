//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
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
        Type TargetHostType { get; }
        Type[] TargetApplicationTypes { get; }
    }

    [AttributeUsage(AttributeTargets.Class), MetadataAttribute]
    public class ModuleAttribute : ExportAttribute, IModuleMetadata
    {
        public Type[] TargetApplicationTypes { get; }
        public Type TargetHostType { get; }

        public ModuleAttribute(Type targetHostType, params Type[] targetAppTypes) 
            : base(typeof(IModule)) 
        {
            TargetHostType = targetHostType;
            TargetApplicationTypes = targetAppTypes;
        }

        public ModuleAttribute(params Type[] targetAppTypes) : this(null, targetAppTypes)
        {
        }

        public ModuleAttribute(Type targetHostType) : this(targetHostType, new Type[0])
        {
        }
    }
}
