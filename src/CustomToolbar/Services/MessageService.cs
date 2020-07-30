//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using Xarial.CadPlus.CustomToolbar.Exceptions;
using Xarial.XCad;
using Xarial.XCad.Base.Enums;

namespace Xarial.CadPlus.CustomToolbar.Services
{
    public enum MessageType_e
    {
        Info,
        Warning,
        Error
    }

    public interface IMessageService
    {
        void ShowMessage(string message, MessageType_e type);
        void ShowError(Exception ex, string baseMsg);
    }

    public class MessageService : IMessageService
    {
        private readonly IXApplication m_App;

        public MessageService(IXApplication app) 
        {
            m_App = app;
        }

        public void ShowMessage(string message, MessageType_e type)
        {
            var icon = MessageBoxIcon_e.Info;

            switch (type)
            {
                case MessageType_e.Info:
                    icon = MessageBoxIcon_e.Info;
                    break;

                case MessageType_e.Warning:
                    icon = MessageBoxIcon_e.Warning;
                    break;

                case MessageType_e.Error:
                    icon = MessageBoxIcon_e.Error;
                    break;
            }

            m_App.ShowMessageBox(message, icon, MessageBoxButtons_e.Ok);
        }

        public void ShowError(Exception ex, string baseMsg)
        {
            if(ex is UserException)
            {
                ShowMessage(ex.Message, MessageType_e.Error);
            }
            else
            {
                ShowMessage(baseMsg, MessageType_e.Error);
            }
        }
    }
}