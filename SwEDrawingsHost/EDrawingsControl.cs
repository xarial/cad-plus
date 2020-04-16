using eDrawings.Interop.EModelViewControl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xarial.XTools.Xport.EDrawingsHost;
using Xarial.XTools.Xport.SwEDrawingsHost;

namespace Xarial.XTools.Xport.EDrawingsHost
{
    public class EDrawingsControl : IPublisher
    {
        private Form m_HostForm;
        private TaskCompletionSource<bool> m_OpenTcs;
        private TaskCompletionSource<bool> m_PrintTcs;
        private TaskCompletionSource<bool> m_SaveTcs;

        private readonly EModelViewControl m_Control;

        public EDrawingsControl()
        {
            m_Control = Load();

            m_Control.OnFinishedLoadingDocument += OnFinishedLoadingDocument;
            m_Control.OnFailedLoadingDocument += OnFailedLoadingDocument;

            m_Control.OnFinishedSavingDocument += OnFinishedSavingDocument;
            m_Control.OnFailedSavingDocument += OnFailedSavingDocument;

            m_Control.OnFinishedPrintingDocument += OnFinishedPrintingDocument;
            m_Control.OnFailedPrintingDocument += OnFailedPrintingDocument;
        }

        public Task OpenDocument(string path)
        {
            m_OpenTcs = new TaskCompletionSource<bool>();
            m_Control.OpenDoc(path, false, false, false, "");
            return m_OpenTcs.Task;
        }

        public Task CloseDocument()
        {
            m_Control.CloseActiveDoc("");
            return Task.CompletedTask;
        }

        public Task SaveDocument(string path)
        {
            var ext = Path.GetExtension(path);

            if (!string.Equals(ext, ".pdf", StringComparison.CurrentCultureIgnoreCase))
            {
                m_SaveTcs = new TaskCompletionSource<bool>();
                m_Control.Save(path, false, "");
                return m_SaveTcs.Task;
            }
            else
            {
                m_PrintTcs = new TaskCompletionSource<bool>();
                var fileName = m_Control.FileName;
                m_Control.Print5(false, fileName, false, false, true, EMVPrintType.eOneToOne, 1, 0, 0, true, 1, 1, path);
                return m_PrintTcs.Task;
            }
        }

        private EModelViewControl Load()
        {
            m_HostForm = new Form();
            var edrwHost = new EDrawingsAxHost();
            m_HostForm.Controls.Add(edrwHost);
            edrwHost.Dock = DockStyle.Fill;
            m_HostForm.ShowIcon = false;
            m_HostForm.ShowInTaskbar = false;
            m_HostForm.WindowState = FormWindowState.Minimized;
            m_HostForm.Show();
            return edrwHost.Control;
        }

        private void OnFinishedLoadingDocument(string fileName)
        {
            m_OpenTcs.SetResult(true);
        }

        private void OnFailedLoadingDocument(string fileName, int errorCode, string errorString)
        {
            m_OpenTcs.SetException(new Exception($"Failed to load document '{fileName}': {errorString}. Error code: {errorCode}"));
        }

        private void OnFinishedSavingDocument()
        {
            m_SaveTcs.SetResult(true);
        }

        private void OnFailedSavingDocument(string fileName, int errorCode, string errorString)
        {
            m_SaveTcs.SetException(new Exception($"Failed to load document '{fileName}': {errorString}. Error code: {errorCode}"));
        }

        private void OnFinishedPrintingDocument(string printJobName)
        {
            m_PrintTcs.SetResult(true);
        }

        private void OnFailedPrintingDocument(string printJobName)
        {
            m_PrintTcs.SetException(new Exception($"Failed to print document '{printJobName}'"));
        }

        public void Dispose()
        {
            m_Control.OnFinishedLoadingDocument -= OnFinishedLoadingDocument;
            m_Control.OnFailedLoadingDocument -= OnFailedLoadingDocument;

            m_Control.OnFinishedSavingDocument -= OnFinishedSavingDocument;
            m_Control.OnFailedSavingDocument -= OnFailedSavingDocument;

            m_Control.OnFinishedPrintingDocument -= OnFinishedPrintingDocument;
            m_Control.OnFailedPrintingDocument -= OnFailedPrintingDocument;

            m_HostForm.Close();
        }
    }
}
