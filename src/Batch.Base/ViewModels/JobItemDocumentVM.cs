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
using Xarial.CadPlus.Batch.Base.Core;
using Xarial.CadPlus.Batch.Base.ViewModels;
using Xarial.CadPlus.Common.Services;
using Xarial.XCad.Documents;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Batch.Base.ViewModels
{
    public class JobItemDocumentVM : JobItemVM
    {
        private readonly JobItemDocument m_JobItemFile;

        public JobItemMacroVM[] Macros { get; }

        public IXDocument Document
            => m_JobItemFile.Document;

        public JobItemDocumentVM(JobItemDocument jobItemFile) : base(jobItemFile)
        {
            m_JobItemFile = jobItemFile;
            Macros = m_JobItemFile.Operations.Select(o => new JobItemMacroVM(o)).ToArray();
        }
    }
}
