//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Batch.Base.ViewModels;
using Xarial.CadPlus.Common.Services;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.XBatch.Base.ViewModels
{
    public class JobItemFileVM : JobItemVM
    {
        private readonly IJobItemDocument m_JobItemFile;

        public JobItemMacroVM[] Macros { get; }

        public JobItemFileVM(IJobItemDocument jobItemFile) : base(jobItemFile)
        {
            m_JobItemFile = jobItemFile;
            Macros = m_JobItemFile.Operations.Select(o => new JobItemMacroVM(o)).ToArray();
        }
    }
}
