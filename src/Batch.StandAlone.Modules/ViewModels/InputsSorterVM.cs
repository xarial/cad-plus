using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Xarial.XCad.Base.Attributes;

namespace Xarial.CadPlus.Batch.StandAlone.Modules.ViewModels
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
        public string FilePath { get; set; }
    }

    public class InputsSorterVM
    {
        public ICollectionView InputView { get; }

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

        public InputsSorterVM(List<ItemVM> input) 
        {
            InputView = CollectionViewSource.GetDefaultView(input);
            InputView.GroupDescriptions.Add(new ItemLevelGroupDescription());
            InputView.Filter = Filter;
            SortType = SortType_e.ChildrenToParents;
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
