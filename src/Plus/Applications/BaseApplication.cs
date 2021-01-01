using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.Applications
{
    public class BaseApplication : IApplication
    {
        public Guid Id { get; }

        public BaseApplication(string id) : this(Guid.Parse(id))
        {
        }

        public BaseApplication(Guid id)
        {
            Id = id;
        }
    }
}
