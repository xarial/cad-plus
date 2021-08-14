//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.ComponentModel;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Batch.StandAlone.ViewModels
{
    public class FilterVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string[] m_Src;
        private int m_Index;
        private string m_Value;

        public string Value
        {
            get => m_Value;
            set 
            {
                m_Value = value;
                m_Src[m_Index] = value;
                this.NotifyChanged();
            }
        }

        public FilterVM() : this("*.*")
        {
        }

        public FilterVM(string value) 
        {
            m_Value = value;
        }

        internal void SetBinding(string[] src, int index) 
        {
            m_Src = src;
            m_Index = index;
        }
    }
}
