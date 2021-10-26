//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Windows;
using System.Windows.Threading;
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Plus.Shared.Services
{
    public class GenericMessageService : IMessageService
    {
        public Type[] UserErrors { get; }
        
        private readonly string m_Title;

        private readonly Dispatcher m_Dispatcher;

        public GenericMessageService() : this("CAD+ Toolset")
        {
        }

        public GenericMessageService(string title, Type[] userErrors) : this(title)
        {
            UserErrors = userErrors;
        }

        public GenericMessageService(string title)
        {
            m_Title = title;
            m_Dispatcher = Dispatcher.CurrentDispatcher;
        }

        public bool? ShowMessage(string msg, MessageServiceIcon_e icon, MessageServiceButtons_e btns)
        {
            MessageBoxImage msgBoxImg;
            MessageBoxButton msgBoxBtns;

            switch (icon) 
            {
                case MessageServiceIcon_e.Information:
                    msgBoxImg = MessageBoxImage.Information;
                    break;

                case MessageServiceIcon_e.Warning:
                    msgBoxImg = MessageBoxImage.Warning;
                    break;

                case MessageServiceIcon_e.Error:
                    msgBoxImg = MessageBoxImage.Error;
                    break;

                case MessageServiceIcon_e.Question:
                    msgBoxImg = MessageBoxImage.Question;
                    break;

                default:
                    throw new NotSupportedException();
            }

            switch (btns) 
            {
                case MessageServiceButtons_e.Ok:
                    msgBoxBtns = MessageBoxButton.OK;
                    break;

                case MessageServiceButtons_e.OkCancel:
                    msgBoxBtns = MessageBoxButton.OKCancel;
                    break;

                case MessageServiceButtons_e.YesNo:
                    msgBoxBtns = MessageBoxButton.YesNo;
                    break;

                case MessageServiceButtons_e.YesNoCancel:
                    msgBoxBtns = MessageBoxButton.YesNoCancel;
                    break;

                default:
                    throw new NotSupportedException();
            }

            switch (ShowMessage(msg, msgBoxImg, msgBoxBtns))
            {
                case MessageBoxResult.Yes:
                case MessageBoxResult.OK:
                    return true;

                case MessageBoxResult.No:
                    return false;

                case MessageBoxResult.Cancel:
                    return null;

                default:
                    throw new NotSupportedException();
            }
        }

        private MessageBoxResult ShowMessage(string msg, MessageBoxImage img, MessageBoxButton btn)
        {
            MessageBoxResult Show() => MessageBox.Show(msg, m_Title, btn, img);

            if (m_Dispatcher != null)
            {
                return m_Dispatcher.Invoke(Show);
            }
            else
            {
                return Show();
            }
        }
    }
}
