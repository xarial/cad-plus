//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Documents;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Batch.Extensions.ViewModels
{
    public enum SortType_e 
    {
        [Title("Children To Parents")]
        ChildrenToParents,

        [Title("Parents To Children")]
        ParentsToChildren,

        [Title("Top Level Only")]
        TopLevelOnly
    }

    public class ItemVM 
    {
        public int Level { get; set; }
        public IXDocument Document { get; set; }
    }

    public class InputsSorterVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ICollectionView m_InputView;

        public ICollectionView InputView 
        {
            get => m_InputView;
            set
            {
                m_InputView = value;
                this.NotifyChanged();
            }
        }

        private SortType_e m_SortType;

        public SortType_e SortType 
        {
            get => m_SortType;
            set 
            {
                m_SortType = value;
                InputView.SortDescriptions.Clear();

                switch (value) 
                {
                    case SortType_e.ChildrenToParents:
                        InputView.SortDescriptions.Add(new SortDescription(nameof(ItemVM.Level), ListSortDirection.Descending));
                        break;

                    case SortType_e.ParentsToChildren:
                        InputView.SortDescriptions.Add(new SortDescription(nameof(ItemVM.Level), ListSortDirection.Ascending));
                        break;
                }
                InputView.Refresh();
            }
        }

        private bool m_IsInitializing;
        private double m_Progress;

        public bool IsInitializing 
        {
            get => m_IsInitializing;
            private set 
            {
                m_IsInitializing = value;
                this.NotifyChanged();
            }
        }

        public double Progress 
        {
            get => m_Progress;
            set 
            {
                m_Progress = value;
                this.NotifyChanged();
            }
        }

        public InputsSorterVM() 
        {
            IsInitializing = true;
        }

        internal void LoadItems(List<ItemVM> input) 
        {
            InputView = CollectionViewSource.GetDefaultView(input);
            InputView.GroupDescriptions.Add(new ItemLevelGroupDescription());
            InputView.Filter = Filter;
            SortType = SortType_e.ChildrenToParents;

            IsInitializing = false;
        }

        private bool Filter(object item)
            => ((ItemVM)item).Level == 0 || SortType != SortType_e.TopLevelOnly;
    }

    public class ItemLevelGroupDescription : GroupDescription
    {
        public override object GroupNameFromItem(object item, int level, CultureInfo culture)
        {
            var itemLevel = ((ItemVM)item).Level;

            if (itemLevel == 0)
            {
                return "Top Level";
            }
            else 
            {
                return $"Level {itemLevel}";
            }
        }
    }
}
