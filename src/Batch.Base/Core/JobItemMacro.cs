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

namespace Xarial.CadPlus.Batch.Base.Core
{
    public class JobItemMacro : IJobItemOperationMacro
    {
        public event JobItemOperationUserResultChangedDelegate UserResultChanged;

        IJobItemOperationDefinition IJobItemOperation.Definition => Definition;
        IJobItemState IJobItemOperation.State => State;
                
        public Exception InternalMacroException { get; set; }

        public JobItemOperationMacroDefinition Definition { get; }
       
        public object UserResult { get; }

        public JobItemState State { get; }

        public JobItemMacro(JobItemOperationMacroDefinition macroDef)
        {
            Definition = macroDef;
            State = new JobItemState();
        }
    }
}
