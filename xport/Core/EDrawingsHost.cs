//*********************************************************************
//xTools
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://xtools.xarial.com
//License: https://xtools.xarial.com/license/
//*********************************************************************

using eDrawings.Interop.EModelViewControl;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xarial.XTools.Xport.UI;

namespace Xarial.XTools.Xport.Core
{
    public class EDrawingsHost : AxHost
    {
        private bool m_IsLoaded;

        private Form m_HostForm;
        private TaskCompletionSource<bool> m_OpenTcs;
        private TaskCompletionSource<bool> m_PrintTcs;
        private TaskCompletionSource<bool> m_SaveTcs;

        private EModelViewControl m_Control;

        public EDrawingsHost() : base("22945A69-1191-4DCF-9E6F-409BDE94D101")
        {
            m_IsLoaded = false;
            Load();

            m_Control.OnFinishedLoadingDocument += OnFinishedLoadingDocument;
            m_Control.OnFailedLoadingDocument += OnFailedLoadingDocument;

            m_Control.OnFinishedSavingDocument += OnFinishedSavingDocument;
            m_Control.OnFailedSavingDocument += OnFailedSavingDocument;

            m_Control.OnFinishedPrintingDocument += OnFinishedPrintingDocument;
            m_Control.OnFailedPrintingDocument += OnFailedPrintingDocument;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            m_Control.OnFinishedLoadingDocument -= OnFinishedLoadingDocument;
            m_Control.OnFailedLoadingDocument -= OnFailedLoadingDocument;

            m_Control.OnFinishedSavingDocument -= OnFinishedSavingDocument;
            m_Control.OnFailedSavingDocument -= OnFailedSavingDocument;

            m_Control.OnFinishedPrintingDocument -= OnFinishedPrintingDocument;
            m_Control.OnFailedPrintingDocument -= OnFailedPrintingDocument;
        }

        public Task OpenDocument(string path) 
        {
            m_OpenTcs = new TaskCompletionSource<bool>();
            m_Control.OpenDoc(path, false, false, false, "");
            return m_OpenTcs.Task;
        }

        public void CloseDocument() 
        {
            m_Control.CloseActiveDoc("");
        }

        public Task SaveDocument(string path) 
        {
            m_SaveTcs = new TaskCompletionSource<bool>();
            m_Control.Save(path, false, "");
            return m_SaveTcs.Task;
        }

        public Task PrintToFile(string printFileName) 
        {
            m_PrintTcs = new TaskCompletionSource<bool>();
            var fileName = m_Control.FileName;
            m_Control.Print5(false, fileName, false, false, true, EMVPrintType.eOneToOne, 1, 0, 0, true, 1, 1, printFileName);
            return m_PrintTcs.Task;
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            if (!m_IsLoaded) //this function is called twice
            {
                m_IsLoaded = true;
                m_Control = GetOcx() as EModelViewControl;

                if (m_Control == null) 
                {
                    throw new Exception("Failed to access eDrawings control");
                }

                const int SIMPLE_UI = 0;
                m_Control.FullUI = SIMPLE_UI;
            }
        }

        private void Load()
        {
            m_HostForm = new Form();
            m_HostForm.Controls.Add(this);
            this.Dock = DockStyle.Fill;
            m_HostForm.ShowIcon = false;
            m_HostForm.ShowInTaskbar = false;
            m_HostForm.WindowState = FormWindowState.Minimized;
            m_HostForm.Show();
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
            m_PrintTcs.SetException(new Exception($"Failed to print document 'printJobName'"));
        }
    }
}
