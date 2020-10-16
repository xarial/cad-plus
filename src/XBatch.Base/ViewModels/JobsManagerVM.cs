using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.XBatch.Base.ViewModels
{
    public class JobsManagerVM
    {
        public IList<JobDocumentVM> JobDocuments { get; }

        public JobsManagerVM()
        {
            JobDocuments = new ObservableCollection<JobDocumentVM>();
        }
    }
}
