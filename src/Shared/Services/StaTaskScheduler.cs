//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;

namespace Xarial.CadPlus.Plus.Shared.Services
{
    public class StaTaskScheduler : TaskScheduler, IDisposable
    {
        private BlockingCollection<Task> m_Tasks;
        private readonly Thread m_Thread;

        private readonly IXLogger m_Logger;

        public StaTaskScheduler(IXLogger logger)
        {
            m_Logger = logger;

            m_Tasks = new BlockingCollection<Task>();

            m_Thread = new Thread(ExecuteTasks)
            {
                IsBackground = true,
                Priority = ThreadPriority.Highest
            };

            m_Thread.SetApartmentState(ApartmentState.STA);

            m_Thread.Start();

            m_Logger.Log($"STA Task Scheduler Thread Id: {m_Thread.ManagedThreadId}", LoggerMessageSeverity_e.Debug);
        }

        private void ExecuteTasks()
        {
            foreach (var task in m_Tasks.GetConsumingEnumerable())
            {
                m_Logger.Log("Executing task", LoggerMessageSeverity_e.Debug);
                TryExecuteTask(task);
            }
        }

        public override int MaximumConcurrencyLevel => 1;

        protected override void QueueTask(Task task) =>
            m_Tasks.Add(task);

        protected override IEnumerable<Task> GetScheduledTasks() =>
            m_Tasks.ToArray();

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) =>
            Thread.CurrentThread.GetApartmentState() == ApartmentState.STA && TryExecuteTask(task);

        public void Dispose()
        {
            if (m_Tasks != null)
            {
                m_Tasks.CompleteAdding();

                m_Thread.Join();

                m_Tasks.Dispose();
                m_Tasks = null;
            }
        }
    }
}
