//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.Attributes
{
    /// <summary>
    /// Defines the hosts where this module can be loaded
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TargetHostAttribute : Attribute
    {
        /// <summary>
        /// Host ids when this module is supported
        /// </summary>
        public Guid[] HostIds { get; }

        /// <summary>
        /// Default attribute
        /// </summary>
        /// <param name="hostIds">Supported host ids</param>
        public TargetHostAttribute(params string[] hostIds) 
        {
            HostIds = hostIds.Select(g => Guid.Parse(g)).ToArray();
        }
    }
}
