//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Xarial.CadPlus.Common.Services
{
    public interface IMessageService
    {
        void ShowError(string error);
        void ShowInformation(string msg);
        bool? ShowQuestion(string question);
    }

    public class MessageService : IMessageService
    {
        public void ShowError(string error) => ShowMessage(error, MessageBoxImage.Error);
        public void ShowInformation(string msg) => ShowMessage(msg, MessageBoxImage.Information);

        public bool? ShowQuestion(string question)
        {
            switch (ShowMessage(question, MessageBoxImage.Information, MessageBoxButton.YesNoCancel)) 
            {
                case MessageBoxResult.Yes:
                    return true;

                case MessageBoxResult.No:
                    return false;

                default:
                    return null;
            }
        }

        private readonly string m_Title;

        public MessageService(string title) 
        {
            m_Title = title;
        }

        public MessageBoxResult ShowMessage(string msg, MessageBoxImage img, MessageBoxButton btn = MessageBoxButton.OK)
            => MessageBox.Show(msg, m_Title, btn, img);
    }
}
