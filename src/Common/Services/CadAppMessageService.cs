//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Diagnostics;
using Xarial.CadPlus.Common.Services;
using Xarial.XCad;
using Xarial.XCad.Base.Enums;

namespace Xarial.CadPlus.Common.Services
{
    public class CadAppMessageService : IMessageService
    {
        private readonly IXApplication m_App;

        public CadAppMessageService(IXApplication app) 
        {
            m_App = app;
        }

        public void ShowError(string error)
            => m_App.ShowMessageBox(error, MessageBoxIcon_e.Error, MessageBoxButtons_e.Ok);

        public void ShowInformation(string msg)
            => m_App.ShowMessageBox(msg, MessageBoxIcon_e.Info, MessageBoxButtons_e.Ok);

        public bool? ShowQuestion(string question)
        {
            var res = m_App.ShowMessageBox(question, MessageBoxIcon_e.Question, MessageBoxButtons_e.YesNoCancel);

            switch (res) 
            {
                case MessageBoxResult_e.Yes:
                    return true;

                case MessageBoxResult_e.No:
                    return false;

                case MessageBoxResult_e.Cancel:
                    return null;

                default:
                    Debug.Assert(false, "Should be 3 options only");
                    return null;
            }
        }
    }
}