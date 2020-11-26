//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.InteropServices;

namespace Xarial.CadPlus.MacroRunner
{
    [ComVisible(true)]
    [Guid("1D7EB548-2651-4656-B205-31F258D81AF1")]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IMacroParameterManager
    {
        IMacroParameter PopParameter(string sessionId);

        void PushParameter(string sessionId, IMacroParameter param);
    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("CF4B61BF-7598-4189-9D5B-D96B7E2587D0")]
    [ProgId("CadPlus.MacroRunner.MacroParameterManager")]
    public class MacroParameterManager : IMacroParameterManager
    {
        private readonly ConcurrentDictionary<string, ConcurrentStack<IMacroParameter>> m_Parameters;

        public MacroParameterManager()
        {
            m_Parameters = new ConcurrentDictionary<string, ConcurrentStack<IMacroParameter>>();
        }

        public IMacroParameter PopParameter(string sessionId)
        {
            if (m_Parameters.TryGetValue(sessionId, out ConcurrentStack<IMacroParameter> paramsStack))
            {
                if (!paramsStack.TryPop(out IMacroParameter param))
                {
                    throw new Exception("Failed to pop the parameter");
                }

                if (!paramsStack.Any())
                {
                    m_Parameters.TryRemove(sessionId, out _);
                }

                return param;
            }
            else
            {
                throw new Exception("Parameter for this macro cannot be found");
            }
        }

        public void PushParameter(string sessionId, IMacroParameter param)
        {
            if (!m_Parameters.TryGetValue(sessionId, out ConcurrentStack<IMacroParameter> paramsStack))
            {
                paramsStack = new ConcurrentStack<IMacroParameter>();
            }

            paramsStack.Push(param);

            if (!m_Parameters.TryAdd(sessionId, paramsStack))
            {
                throw new Exception("Failed to push the parameter");
            }
        }
    }
}
