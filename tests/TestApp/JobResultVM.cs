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
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Extensions;

namespace TestApp
{
    public class MyAsyncBatchJob : IAsyncBatchJob
    {
        public event JobSetDelegate JobSet;
        public event JobCompletedDelegate JobCompleted;
        public event JobItemProcessedDelegate ItemProcessed;
        public event JobLogDelegateDelegate Log;

        public IJobItem[] JobItems { get; private set; }

        public async Task<bool> ExecuteAsync(CancellationToken cancellationToken)
        {
            async Task ProcessJobItemOperation(MyJobItemOperation oper, JobItemState_e res, object userRes, string[] issues) 
            {
                oper.Update(JobItemState_e.Initializing, null, null);
                await Task.Delay(TimeSpan.FromSeconds(1));
                oper.Update(JobItemState_e.InProgress, null, null);
                await Task.Delay(TimeSpan.FromSeconds(2));
                oper.Update(res, issues?.Select(i => new MyJobItemIssue(res == JobItemState_e.Failed ? IssueType_e.Error : IssueType_e.Information, i)).ToArray(), userRes);
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

            JobSet?.Invoke(this, new IJobItem[] { item1, item2, item3 }, new IJobItemOperationDefinition[] { oper1, oper2 }, startTime);

            try
            {
                Log?.Invoke(this, "Processing item1oper1");
                await ProcessJobItemOperation(item1oper1, JobItemState_e.Succeeded, null, null);

                Log?.Invoke(this, "Processing item1oper2");
                await ProcessJobItemOperation(item1oper2, JobItemState_e.Succeeded, null, null);

                ItemProcessed?.Invoke(this, item1, 1d / 3d, true);

                Log?.Invoke(this, "Processing item2oper1");
                await ProcessJobItemOperation(item2oper1, JobItemState_e.Failed, null, new string[] { "Some Error 1", "Some Error 2" });

                Log?.Invoke(this, "Processing item2oper2");
                await ProcessJobItemOperation(item2oper2, JobItemState_e.Succeeded, "Test Result", new string[] { "Some Info 1" });

                ItemProcessed?.Invoke(this, item2, 2d / 3d, true);

                Log?.Invoke(this, "Processing item3oper1");
                await ProcessJobItemOperation(item3oper1, JobItemState_e.Failed, null, null);

                Log?.Invoke(this, "Processing item3oper2");
                await ProcessJobItemOperation(item3oper2, JobItemState_e.Failed, null, null);

                ItemProcessed?.Invoke(this, item1, 3d / 3d, true);

                return true;
            }
            catch
            {
                return false;
            }
            finally 
            {
                JobCompleted?.Invoke(this, DateTime.Now.Subtract(startTime));
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

    public class MyJobItem : IJobItem
    {
        public ImageSource Icon { get; }
        public ImageSource Preview { get; }
        public string Title { get; }
        public string Description { get; }
        public Action Link { get; }
        public IJobItemOperation[] Operations { get; }
        public IJobItem[] Nested { get; }

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
        }
    }

    public class MyJobItemOperation : IJobItemOperation
    {
        public event JobItemOperationStateChangedDelegate StateChanged;
        public event JobItemOperationIssuesChangedDelegate IssuesChanged;
        public event JobItemOperationUserResultChangedDelegate UserResultChanged;

        public IJobItemOperationDefinition Definition { get; }
        
        public JobItemState_e State { get; private set; }
        public IJobItemIssue[] Issues { get; private set; }
        public object UserResult { get; private set; }

        public MyJobItemOperation(IJobItemOperationDefinition def) 
        {
            Definition = def;
        }

        public void Update(JobItemState_e state, IJobItemIssue[] issues, object userRes) 
        {
            State = state;
            Issues = issues;
            UserResult = userRes;

            StateChanged?.Invoke(this, State);
            IssuesChanged?.Invoke(this, Issues);
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

        private JobResultBaseVM m_Result;

        private CancellationTokenSource m_CancellationTokenSource;

        public JobResultVM() 
        {
            RunJobCommand = new RelayCommand(RunJob);
        }

        private async void RunJob()
        {
            m_CancellationTokenSource = new CancellationTokenSource();

            var res = new AsyncJobResultVM("Test Async Job", new MyAsyncBatchJob(), Mock.Of<IXLogger>(), m_CancellationTokenSource);

            Result = res;

            await res.TryRunBatchAsync();
        }
    }
}
