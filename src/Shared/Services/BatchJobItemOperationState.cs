using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Extensions;
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Plus.Shared.Services
{
    public class BatchJobItemOperationState : BatchJobItemStateBase, IBatchJobItemOperationState
    {
        public event BatchJobItemOperationStateStatusChangedDelegate StatusChanged;
        public event BatchJobItemOperationStateIssuesChangedDelegate IssuesChanged;

        private readonly IBatchJobItem m_Item;
        private readonly IBatchJobItemOperation m_Operation;

        public BatchJobItemOperationState(IBatchJobItem item, IBatchJobItemOperation operation)
        {
            m_Item = item;
            m_Operation = operation;
        }

        protected override void RaiseIssuesChanged(IReadOnlyList<IBatchJobItemIssue> issues)
            => IssuesChanged?.Invoke(this, m_Item, m_Operation, issues);

        protected override void RaiseStatusChanged(BatchJobItemStateStatus_e status)
            => StatusChanged?.Invoke(this, m_Item, m_Operation, status);
    }
}
