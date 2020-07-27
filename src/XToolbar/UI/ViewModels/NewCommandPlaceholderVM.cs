using System;
using System.Windows.Input;
using Xarial.XToolkit.Wpf;

namespace Xarial.CadPlus.XToolbar.UI.ViewModels
{
    public class NewCommandPlaceholderVM
    {
        public event Action AddNewCommand;

        private ICommand m_AddNewItemCommand;

        public ICommand AddNewItemCommand
        {
            get
            {
                if (m_AddNewItemCommand == null)
                {
                    m_AddNewItemCommand = new RelayCommand(
                        () =>
                        {
                            AddNewCommand?.Invoke();
                        });
                }

                return m_AddNewItemCommand;
            }
        }
    }
}