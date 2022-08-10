using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TestApp.Properties;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Shared.ViewModels;
using Xarial.XCad.Base;
using Xarial.XToolkit.Services;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Extensions;

namespace TestApp
{
    public class MyAsyncBatchJob : IAsyncBatchJob
    {
        public event JobInitializedDelegate Initialized;
        public event JobCompletedDelegate Completed;
        public event JobItemProcessedDelegate ItemProcessed;
        public event JobLogDelegateDelegate Log;
        public event JobProgressChangedDelegate ProgressChanged;

        public IReadOnlyList<IJobItem> JobItems => m_JobItems;
        public IReadOnlyList<string> LogEntries => m_LogEntries;
        public IReadOnlyList<IJobItemOperationDefinition> OperationDefinitions => m_OperationDefinitions;

        private List<IJobItem> m_JobItems;
        private List<string> m_LogEntries;
        private List<IJobItemOperationDefinition> m_OperationDefinitions;

        public MyAsyncBatchJob() 
        {
            m_JobItems = new List<IJobItem>();
            m_LogEntries = new List<string>();
            m_OperationDefinitions = new List<IJobItemOperationDefinition>();
        }

        public async Task<bool> ExecuteAsync(CancellationToken cancellationToken)
        {
            async Task ProcessJobItemOperation(MyJobItemOperation oper, JobItemStateStatus_e res, object userRes, string[] issues) 
            {
                cancellationToken.ThrowIfCancellationRequested();
                oper.Update(JobItemStateStatus_e.Initializing, null, null);
                await Task.Delay(TimeSpan.FromSeconds(1));
                cancellationToken.ThrowIfCancellationRequested();
                oper.Update(JobItemStateStatus_e.InProgress, null, null);
                await Task.Delay(TimeSpan.FromSeconds(2));
                cancellationToken.ThrowIfCancellationRequested();
                oper.Update(res, issues?.Select(i => new MyJobItemIssue(res == JobItemStateStatus_e.Failed ? IssueType_e.Error : IssueType_e.Information, i)).ToArray(), userRes);
            }

            var oper1 = new MyJobItemOperationDefinition("Operation #1", Resources.icon1);
            var oper2 = new MyJobItemOperationDefinition("Operation #2", Resources.icon2);

            var item1oper1 = new MyJobItemOperation(oper1);
            var item1oper2 = new MyJobItemOperation(oper2);

            var item2oper1 = new MyJobItemOperation(oper1);
            var item2oper2 = new MyJobItemOperation(oper2);

            var item3oper1 = new MyJobItemOperation(oper1);
            var item3oper2 = new MyJobItemOperation(oper2);

            var item1 = new MyJobItem(Resources.icon3, Resources.preview, "Item1", "First Item", null, new IJobItemOperation[] { item1oper1, item1oper2 }, null);
            var item2 = new MyJobItem(Resources.icon4, null, "Item2", "Second Item", () => MessageBox.Show("Item2 is clicked"), new IJobItemOperation[] { item2oper1, item2oper2 }, null);
            var item3 = new MyJobItem(Resources.icon3, null, "Item3", "Third Item", null, new IJobItemOperation[] { item3oper1, item3oper2 }, null);

            var startTime = DateTime.Now;

            //Initializing
            await Task.Delay(TimeSpan.FromSeconds(2));

            m_JobItems.AddRange(new IJobItem[] { item1, item2, item3 });

            m_OperationDefinitions.AddRange(new IJobItemOperationDefinition[] { oper1, oper2 });

            Initialized?.Invoke(this, m_JobItems, m_OperationDefinitions, startTime);

            try
            {
                //item1
                item1.Update(JobItemStateStatus_e.InProgress, null, null);

                Log?.Invoke(this, "Processing item1oper1");
                await ProcessJobItemOperation(item1oper1, JobItemStateStatus_e.Succeeded, null, null);

                Log?.Invoke(this, "Processing item1oper2");
                await ProcessJobItemOperation(item1oper2, JobItemStateStatus_e.Succeeded, null, null);

                item1.Update(item1.ComposeStatus(), null, null);

                ItemProcessed?.Invoke(this, item1, true);
                ProgressChanged?.Invoke(this, 1d / 3d);

                //item2
                item2.Update(JobItemStateStatus_e.InProgress, null, null);

                Log?.Invoke(this, "Processing item2oper1");
                await ProcessJobItemOperation(item2oper1, JobItemStateStatus_e.Failed, "Failed Result", new string[] { "Some Error 1", "Some Error 2" });

                Log?.Invoke(this, "Processing item2oper2");
                await ProcessJobItemOperation(item2oper2, JobItemStateStatus_e.Succeeded, "Test Result", new string[] { "Some Info 1" });

                item2.Update(item2.ComposeStatus(), new IJobItemIssue[] { new MyJobItemIssue(IssueType_e.Warning, "Some Warning") }, null);

                ItemProcessed?.Invoke(this, item2, true);
                ProgressChanged?.Invoke(this, 2d / 3d);

                //item3
                item3.Update(JobItemStateStatus_e.InProgress, null, null);

                Log?.Invoke(this, "Processing item3oper1");
                await ProcessJobItemOperation(item3oper1, JobItemStateStatus_e.Failed, null, null);

                Log?.Invoke(this, "Processing item3oper2");
                await ProcessJobItemOperation(item3oper2, JobItemStateStatus_e.Failed, null, null);

                item3.Update(item3.ComposeStatus(), null, null);

                ItemProcessed?.Invoke(this, item3, true);
                ProgressChanged?.Invoke(this, 3d / 3d);

                return true;
            }
            catch
            {
                return false;
            }
            finally 
            {
                Completed?.Invoke(this, DateTime.Now.Subtract(startTime));
            }
        }

        public void Dispose()
        {
        }
    }

    public class MyJobItemIssue : IJobItemIssue
    {
        public IssueType_e Type { get; }
        public string Content { get; }

        public MyJobItemIssue(IssueType_e type, string content)
        {
            Type = type;
            Content = content;
        }
    }

    public class MyJobItemOperationDefinition : IJobItemOperationDefinition
    {
        public string Name { get; }
        public ImageSource Icon { get; }

        public MyJobItemOperationDefinition(string name, Image icon)
        {
            Name = name;
            Icon = icon.ToBitmapImage();
        }
    }

    public class MyJobItemState : IJobItemState
    {
        public event JobStateStatusChangedDelegate StatusChanged;
        public event JobStateIssuesChangedDelegate IssuesChanged;

        public JobItemStateStatus_e Status { get; private set; }
        public IReadOnlyList<IJobItemIssue> Issues { get; private set; }

        public void Update(JobItemStateStatus_e status, IJobItemIssue[] issues)
        {
            Status = status;
            Issues = issues;

            StatusChanged?.Invoke(this, Status);
            IssuesChanged?.Invoke(this, Issues);
        }
    }

    public class MyJobItem : IJobItem
    {
        public event JobItemNestedItemsInitializedDelegate NestedItemsInitialized;

        public ImageSource Icon { get; }
        public ImageSource Preview { get; }
        public string Title { get; }
        public string Description { get; }
        public Action Link { get; }
        public IReadOnlyList<IJobItemOperation> Operations { get; }
        public IReadOnlyList<IJobItem> Nested { get; }

        public IJobItemState State => m_State;

        private readonly MyJobItemState m_State;

        public MyJobItem(Image icon, Image preview, string title, string description,
            Action link, IJobItemOperation[] operations, IJobItem[] nested)
        {
            Icon = icon?.ToBitmapImage();
            Preview = preview?.ToBitmapImage();
            Title = title;
            Description = description;
            Link = link;
            Operations = operations;
            Nested = nested;

            m_State = new MyJobItemState();
        }

        public void Update(JobItemStateStatus_e status, IJobItemIssue[] issues, object userRes)
        {
            m_State.Update(status, issues);
        }
    }

    public class MyJobItemOperation : IJobItemOperation
    {
        
        public event JobItemOperationUserResultChangedDelegate UserResultChanged;

        public IJobItemOperationDefinition Definition { get; }
        
        public object UserResult { get; private set; }

        public IJobItemState State => m_State;

        private MyJobItemState m_State;

        public MyJobItemOperation(IJobItemOperationDefinition def) 
        {
            Definition = def;
            m_State = new MyJobItemState();
        }

        public void Update(JobItemStateStatus_e status, IJobItemIssue[] issues, object userRes) 
        {
            m_State.Update(status, issues);

            UserResult = userRes;
            UserResultChanged?.Invoke(this, UserResult);
        }
    }

    public class JobResultVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public JobResultBaseVM Result 
        {
            get => m_Result;
            private set 
            {
                m_Result = value;
                this.NotifyChanged();
            }
        }

        public ICommand RunJobCommand { get; }
        public ICommand CancelJobCommand { get; }

        private JobResultBaseVM m_Result;

        private CancellationTokenSource m_CancellationTokenSource;

        public JobResultVM() 
        {
            RunJobCommand = new RelayCommand(RunJob, () => m_CancellationTokenSource == null);
            CancelJobCommand = new RelayCommand(CancelJob, () => m_CancellationTokenSource != null);
        }

        private async void RunJob()
        {
            m_CancellationTokenSource = new CancellationTokenSource();

            var res = new AsyncJobResultVM(new MyAsyncBatchJob(), Mock.Of<IMessageService>(), Mock.Of<IXLogger>(), m_CancellationTokenSource, null, null);

            Result = res;

            await res.TryRunBatchAsync();
        }

        private void CancelJob()
        {
            m_CancellationTokenSource.Cancel();
            m_CancellationTokenSource = null;
        }
    }
}
