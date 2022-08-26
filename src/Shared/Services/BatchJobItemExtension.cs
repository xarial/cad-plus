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
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Plus.Shared.Services
{
    public static class BatchJobItemExtension
    {
        public static BatchJobItemStateStatus_e ComposeStatus(this IBatchJobItem jobItem)
        {
            var statuses = jobItem.Operations.Select(o => o.State.Status).ToArray();

            if (statuses.All(s => s == BatchJobItemStateStatus_e.Queued))
            {
                return BatchJobItemStateStatus_e.Queued;
            }
            else if (statuses.All(s => s == BatchJobItemStateStatus_e.Succeeded))
            {
                if (jobItem.State.Issues?.Any(i => i.Type == BatchJobItemIssueType_e.Error) == true)
                {
                    return BatchJobItemStateStatus_e.Succeeded;
                }
                else if (jobItem.State.Issues?.Any(i => i.Type == BatchJobItemIssueType_e.Warning) == true)
                {
                    return BatchJobItemStateStatus_e.Warning;
                }
                else
                {
                    return BatchJobItemStateStatus_e.Succeeded;
                }
            }
            else if (statuses.All(s => s == BatchJobItemStateStatus_e.Failed))
            {
                return BatchJobItemStateStatus_e.Failed;
            }
            else if (statuses.All(s => s == BatchJobItemStateStatus_e.Failed || s == BatchJobItemStateStatus_e.Succeeded || s == BatchJobItemStateStatus_e.Warning))
            {
                return BatchJobItemStateStatus_e.Warning;
            }
            else
            {
                return BatchJobItemStateStatus_e.InProgress;
            }
        }
    }
}
