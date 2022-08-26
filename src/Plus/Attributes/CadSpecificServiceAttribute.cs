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

namespace Xarial.CadPlus.Plus.Attributes
{
    /// <summary>
    /// Indicates that this service is a specific to the CAD
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CadSpecificServiceAttribute : Attribute
    {
        /// <summary>
        /// Id of the CAD system from <see cref="CadApplicationIds"/>
        /// </summary>
        public string CadId { get; }

        public CadSpecificServiceAttribute(string cadId) 
        {
            CadId = cadId;
        }
    }
}
