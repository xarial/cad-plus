//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.Windows;

namespace Xarial.CadPlus.Xport.Services
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

        public void ShowMessage(string msg, MessageBoxImage img, MessageBoxButton btn = MessageBoxButton.OK) 
        {
            MessageBox.Show(msg, "xPort", btn, img);
        }
    }
}
