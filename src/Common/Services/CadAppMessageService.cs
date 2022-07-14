//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using Xarial.CadPlus.Plus.Services;
using Xarial.XCad;
using Xarial.XCad.Base.Enums;
using Xarial.XToolkit.Services;

namespace Xarial.CadPlus.Common.Services
{
    public class CadAppMessageService : IMessageService
    {
        private readonly IXApplication m_App;

        public Type[] UserErrors { get; }

        public CadAppMessageService(IXApplication app, Type[] userErrors)
        {
            m_App = app;
            UserErrors = userErrors;
        }

        public bool? ShowMessage(string msg, MessageServiceIcon_e icon, MessageServiceButtons_e btns)
        {
            MessageBoxIcon_e msgBoxImg;
            MessageBoxButtons_e msgBoxBtns;

            switch (icon)
            {
                case MessageServiceIcon_e.Information:
                    msgBoxImg = MessageBoxIcon_e.Info;
                    break;

                case MessageServiceIcon_e.Warning:
                    msgBoxImg = MessageBoxIcon_e.Warning;
                    break;

                case MessageServiceIcon_e.Error:
                    msgBoxImg = MessageBoxIcon_e.Error;
                    break;

                case MessageServiceIcon_e.Question:
                    msgBoxImg = MessageBoxIcon_e.Question;
                    break;

                default:
                    throw new NotSupportedException();
            }

            switch (btns)
            {
                case MessageServiceButtons_e.Ok:
                    msgBoxBtns = MessageBoxButtons_e.Ok;
                    break;

                case MessageServiceButtons_e.OkCancel:
                    msgBoxBtns = MessageBoxButtons_e.OkCancel;
                    break;

                case MessageServiceButtons_e.YesNo:
                    msgBoxBtns = MessageBoxButtons_e.YesNo;
                    break;

                case MessageServiceButtons_e.YesNoCancel:
                    msgBoxBtns = MessageBoxButtons_e.YesNoCancel;
                    break;

                default:
                    throw new NotSupportedException();
            }

            return ShowMessage(msg, msgBoxImg, msgBoxBtns);
        }

        private bool? ShowMessage(string question, MessageBoxIcon_e img, MessageBoxButtons_e btns)
        {
            var res = m_App.ShowMessageBox(question, img, btns);

            switch (res)
            {
                case MessageBoxResult_e.Yes:
                case MessageBoxResult_e.Ok:
                    return true;

                case MessageBoxResult_e.No:
                    return false;

                case MessageBoxResult_e.Cancel:
                    return null;

                default:
                    throw new NotSupportedException();
            }
        }
    }
}