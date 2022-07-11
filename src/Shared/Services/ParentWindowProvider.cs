using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Plus.Shared.Services
{
    public class ParentWindowProvider : IParentWindowProvider
    {
        public Window Window => m_WindowFactory?.Invoke();
        public IntPtr Handle => m_HandleFactory?.Invoke() ?? IntPtr.Zero;

        private readonly Func<Window> m_WindowFactory;
        private readonly Func<IntPtr> m_HandleFactory;

        public ParentWindowProvider(Window window) : this(() => window)
        {
        }

        public ParentWindowProvider(Func<Window> windowFact)
        {
            m_WindowFactory = windowFact;
        }

        public ParentWindowProvider(IntPtr handle) : this(() => handle)
        {
        }

        public ParentWindowProvider(Func<IntPtr> handleFact)
        {
            m_HandleFactory = handleFact;
        }
    }
}
