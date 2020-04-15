using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Xarial.XTools.Xport.UI
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> m_ExecuteFunc = null;
        private readonly Predicate<T> m_CanExecuteFunc = null;

        public RelayCommand(Action<T> executeFunc, Predicate<T> canExecuteFunc = null)
        {
            if (executeFunc == null)
            {
                throw new ArgumentNullException(nameof(executeFunc));
            }

            m_ExecuteFunc = executeFunc;
            m_CanExecuteFunc = canExecuteFunc;
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public bool CanExecute(object parameter)
        {
            return m_CanExecuteFunc == null
                || m_CanExecuteFunc((T)parameter);
        }

        public void Execute(object parameter)
        {
            m_ExecuteFunc.Invoke((T)parameter);
        }
    }

    public class RelayCommand : RelayCommand<object>
    {
        public RelayCommand(Action executeFunc, Func<bool> canExecuteFunc = null)
            : base(new Action<object>(a => executeFunc.Invoke()),
                  new Predicate<object>(a => canExecuteFunc == null ? true : canExecuteFunc.Invoke()))
        {
        }
    }
}
