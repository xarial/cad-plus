using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Data;

namespace Xarial.CadPlus.Plus.Services
{
    public enum EntityType_e
    {
        Part,
        Assembly,
        Drawing,
        Configuration,
        CutList
    }

    public interface ICadEntityDescriptor
    {
        Image GetIcon(EntityType_e entType);
        string GetTitle(EntityType_e entType);
        FileTypeFilter[] GetFileFilters(EntityType_e entType);
    }
}
