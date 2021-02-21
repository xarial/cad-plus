using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Batch.Extensions.ViewModels
{
    public class ReferenceExtractorVM
    {
        public ReferenceVM[] References { get; }
        public ObservableCollection<string> AdditionalDrawingFolders { get; }

        public ReferenceExtractorVM(ReferenceVM[] refs) 
        {
            References = refs;
            AdditionalDrawingFolders = new ObservableCollection<string>();
        }
    }
}
