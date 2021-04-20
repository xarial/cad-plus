//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using Polly;
using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Batch.Base.Services
{
    public interface IResilientWorker<TContext>
    {
        event Action<Exception, int, TContext> Retry;
        event Action<TContext> Timeout;

        void DoWork(Action<TContext, CancellationToken> action, TContext context, CancellationToken cancellationToken);
    }

    public class PollyResilientWorker<TContext> : IResilientWorker<TContext>
    {
        public event Action<Exception, int, TContext> Retry;
        public event Action<TContext> Timeout;

        private readonly ISyncPolicy m_Policy;

        private const string CONTEXT_PARAM_NAME = "_Context_";

        public PollyResilientWorker(int retries, TimeSpan? timeout) 
        {
            m_Policy = Policy
                    .Handle<Exception>()
                    .Retry(retries, OnRetry);

            if (timeout.HasValue)
            {
                var timeoutPolicy = Policy.Timeout(timeout.Value,
                    TimeoutStrategy.Pessimistic, OnTimeout);

                m_Policy = m_Policy.Wrap(timeoutPolicy);
            }
        }

        public void DoWork(Action<TContext, CancellationToken> action, TContext context, CancellationToken cancellationToken)
        {
            m_Policy.Execute((Context ctx, CancellationToken token) => 
            {
                action.Invoke(GetContext(ctx), token);
            }, new Dictionary<string, object>() { { CONTEXT_PARAM_NAME, context } }, cancellationToken);
        }

        private TContext GetContext(Context ctx) => (TContext)ctx[CONTEXT_PARAM_NAME];

        private void OnTimeout(Context context, TimeSpan timeout, Task task)
            => Timeout?.Invoke(GetContext(context));

        private void OnRetry(Exception err, int attempt, Context context)
            => Retry?.Invoke(err, attempt, GetContext(context));
    }
}
