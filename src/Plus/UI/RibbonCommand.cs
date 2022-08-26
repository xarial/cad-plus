//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.UI
{
    public interface IRibbonCommand
    {
        Image Icon { get; }
        string Title { get; }
        string Description { get; }
        void Update();
    }

    public abstract class RibbonCommand : IRibbonCommand, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Image Icon { get; }
        public string Title { get; }
        public string Description { get; }

        public RibbonCommand(string title, Image icon, string description)
        {
            Title = title;
            Icon = icon;
            Description = description;
        }

        public virtual void Update()
        {
            NotifyChanged(nameof(Icon));
            NotifyChanged(nameof(Title));
            NotifyChanged(nameof(Description));
        }

        protected void NotifyChanged(string prpName)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prpName));
    }
}
