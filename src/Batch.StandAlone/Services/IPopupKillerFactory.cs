using System;
using System.Diagnostics;
using Xarial.CadPlus.Plus.Shared.Services;
using Xarial.XCad.Base;

namespace Xarial.CadPlus.Batch.StandAlone.Services
{
    public interface IPopupKillerFactory 
    {
        IPopupKiller Create();
    }

    public class PopupKillerFactory : IPopupKillerFactory
    {
        private readonly IXLogger m_Logger;

        public PopupKillerFactory(IXLogger logger) 
        {
            m_Logger = logger;
        }

        public IPopupKiller Create() => new PopupKiller(m_Logger);
    }
}
