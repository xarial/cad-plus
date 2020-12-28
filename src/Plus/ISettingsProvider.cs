using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus
{
    public interface ISettingsProvider
    {
        T ReadSettings<T>()
            where T : new();

        void WriteSettings<T>(T setts)
            where T : new();
    }
}
