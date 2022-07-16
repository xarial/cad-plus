//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
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
using Xarial.CadPlus.Batch.Base.Core;
using Xarial.CadPlus.Batch.StandAlone.Services;
using Xarial.CadPlus.Plus.Exceptions;

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

        private readonly int m_Retries;

        public PollyResilientWorker(int retries, TimeSpan? timeout) 
        {
            m_Retries = retries;

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
            try
            {
                var res = m_Policy.ExecuteAndCapture((Context ctx, CancellationToken token) =>
                {
                    action.Invoke(GetContext(ctx), token);
                }, new Dictionary<string, object>() { { CONTEXT_PARAM_NAME, context } }, cancellationToken);

                if (res.Outcome == OutcomeType.Failure)
                {
                    if (res.FinalException != null)
                    {
                        throw new UserException($"Failed to process the operation within {m_Retries} retries", res.FinalException);
                    }
                    else 
                    {
                        throw new Exception("Unknown error");
                    }
                }
            }
            catch (TimeoutRejectedException ex) 
            {
                throw new TimeoutException("Timeout", ex);
            }
        }

        private TContext GetContext(Context ctx) => (TContext)ctx[CONTEXT_PARAM_NAME];

        private void OnTimeout(Context context, TimeSpan timeout, Task task)
            => Timeout?.Invoke(GetContext(context));

        private void OnRetry(Exception err, int attempt, Context context)
            => Retry?.Invoke(err, attempt, GetContext(context));
    }

    public class PollyJobContectResilientWorkerFactory : IJobContectResilientWorkerFactory
    {
        private readonly int m_Retries;

        public PollyJobContectResilientWorkerFactory(int retries) 
        {
            m_Retries = retries;
        }

        public IResilientWorker<BatchJobContext> Create(TimeSpan? timeout)
            => new PollyResilientWorker<BatchJobContext>(m_Retries, timeout);
    }
}
