//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Windows.Input;
using Xarial.XToolkit.Wpf;

namespace Xarial.CadPlus.CustomToolbar.UI.ViewModels
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