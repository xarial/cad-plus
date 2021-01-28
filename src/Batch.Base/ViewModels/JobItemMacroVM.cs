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
    public class JobItemMacroVM : JobItemVM
    {
        private readonly IJobItemOperation m_JobItemMacro;

        public JobItemMacroVM(IJobItemOperation jobItemMacro) : base(jobItemMacro)
        {
            m_JobItemMacro = jobItemMacro;
        }
    }
}
