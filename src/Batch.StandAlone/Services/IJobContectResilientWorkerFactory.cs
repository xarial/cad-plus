//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using Xarial.CadPlus.Batch.Base.Core;
using Xarial.CadPlus.Batch.Base.Services;

namespace Xarial.CadPlus.Batch.StandAlone.Services
{
    public interface IJobContectResilientWorkerFactory 
    {
        IResilientWorker<BatchJobContext> Create(TimeSpan? timeout);
    }
}
