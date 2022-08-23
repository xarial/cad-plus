using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Extensions;
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Plus.Shared.Services
{
    public class BatchJobItemState : BatchJobItemStateBase, IBatchJobItemState
    {
        public event BatchJobItemStateStatusChangedDelegate StatusChanged;
        public event BatchJobItemStateIssuesChangedDelegate IssuesChanged;

        private readonly IBatchJobItem m_Item;

        public BatchJobItemState(IBatchJobItem item)
        {
            m_Item = item;
        }

        protected override void RaiseIssuesChanged(IReadOnlyList<IBatchJobItemIssue> issues)
            => IssuesChanged?.Invoke(this, m_Item, issues);

        protected override void RaiseStatusChanged(BatchJobItemStateStatus_e status)
            => StatusChanged?.Invoke(this, m_Item, status);
    }
}
