using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XTools.Xport.SwEDrawingsHost
{
    public interface IPublisher : IDisposable
    {
        Task OpenDocument(string path);
        Task SaveDocument(string path);
        Task CloseDocument();
    }
}
