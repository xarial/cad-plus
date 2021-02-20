using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Data;

namespace Xarial.CadPlus.Plus.Services
{
    public interface ICadEntityDescriptor
    {
        string ApplicationId { get; }
        string ApplicationName { get; }
        Image ApplicationIcon { get; }

        Image PartIcon { get; }
        Image AssemblyIcon { get; }
        Image DrawingIcon { get; }
        Image ConfigurationIcon { get; }
        Image SheetIcon { get; }
        Image CutListIcon { get; }

        FileTypeFilter PartFileFilter { get; }
        FileTypeFilter AssemblyFileFilter { get; }
        FileTypeFilter DrawingFileFilter { get; }

        FileTypeFilter[] MacroFileFilters { get; }
    }
}
