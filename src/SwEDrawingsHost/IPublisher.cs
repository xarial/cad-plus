//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Xport.SwEDrawingsHost
{
    public interface IPublisher : IDisposable
    {
        Task OpenDocument(string path);

        Task SaveDocument(string path);

        Task CloseDocument();
    }
}