using System;
using Xarial.CadPlus.CustomToolbar.Base;
using Xarial.XCad;

namespace Xarial.CadPlus.Toolbar.Services
{
    public class LambdaStateResolver : IToggleButtonStateResolver
    {
        public IXApplication Application { get; }

        private Delegate m_Invoker;
        private object[] m_Args;

        public LambdaStateResolver(IXApplication app, Delegate invoker) 
        {
            Application = app;
            m_Invoker = invoker;
            m_Args = new object[] { app };
        }

        public bool Resolve() => (bool)m_Invoker.DynamicInvoke(m_Args);
    }
}
