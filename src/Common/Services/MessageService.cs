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
    }

    public class MessageService : IMessageService
    {
        public void ShowError(string error) => ShowMessage(error, MessageBoxImage.Error);
        public void ShowInformation(string msg) => ShowMessage(msg, MessageBoxImage.Information);

        private readonly string m_Title;

        public MessageService(string title) 
        {
            m_Title = title;
        }

        public void ShowMessage(string msg, MessageBoxImage img, MessageBoxButton btn = MessageBoxButton.OK)
        {
            MessageBox.Show(msg, m_Title, btn, img);
        }
    }
}
