//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xarial.CadPlus.Batch.Base.Services;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Plus.Extensions;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Shared.Services;

namespace Xarial.CadPlus.Batch.Base.Core
{
    public class JobItemMacro : IJobItemOperationMacro
    {
        public event BatchJobItemOperationUserResultChangedDelegate UserResultChanged;

        IBatchJobItemOperationDefinition IBatchJobItemOperation.Definition => Definition;
        IBatchJobItemOperationState IBatchJobItemOperation.State => State;
                
        public Exception InternalMacroException { get; set; }

        public JobItemOperationMacroDefinition Definition { get; }
       
        public object UserResult { get; }

        public BatchJobItemOperationState State { get; }

        private readonly JobItemDocument m_ItemDoc;

        public JobItemMacro(JobItemDocument itemDoc, JobItemOperationMacroDefinition macroDef)
        {
            Definition = macroDef;
            State = new BatchJobItemOperationState(m_ItemDoc, this);
        }
    }
}
