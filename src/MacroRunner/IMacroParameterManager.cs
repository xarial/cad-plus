//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Xarial.CadPlus.MacroRunner
{
    [ComVisible(true)]
    [Guid("1D7EB548-2651-4656-B205-31F258D81AF1")]
    public interface IMacroParameterManager
    {
        IMacroParameter PopParameter(string sessionId);

        void PushParameter(string sessionId, IMacroParameter param);

        void TryRemoveParameter(string sessionId, IMacroParameter param);
    }

    [ComVisible(true)]
    [Guid("CF4B61BF-7598-4189-9D5B-D96B7E2587D0")]
    [ProgId("CadPlus.MacroRunner.MacroParameterManager")]
    public class MacroParameterManager : IMacroParameterManager
    {
        private readonly Dictionary<string, List<IMacroParameter>> m_Parameters;

        private readonly object m_Lock;

        public MacroParameterManager()
        {
            m_Parameters = new Dictionary<string, List<IMacroParameter>>();
            m_Lock = new object();
        }

        public IMacroParameter PopParameter(string sessionId)
        {
            lock (m_Lock)
            {
                if (m_Parameters.TryGetValue(sessionId, out List<IMacroParameter> paramsStack))
                {
                    if (paramsStack.Count > 0)
                    {
                        var param = paramsStack.First();

                        paramsStack.Remove(param);

                        if (!paramsStack.Any())
                        {
                            m_Parameters.Remove(sessionId);
                        }

                        return param;
                    }
                    else
                    {
                        throw new Exception("No parameters remain for this session");
                    }
                }
                else
                {
                    throw new Exception("Parameter for this macro cannot be found");
                }
            }
        }

        public void PushParameter(string sessionId, IMacroParameter param)
        {
            lock (m_Lock)
            {
                if (!m_Parameters.TryGetValue(sessionId, out List<IMacroParameter> paramsStack))
                {
                    paramsStack = new List<IMacroParameter>();
                    m_Parameters.Add(sessionId, paramsStack);
                }

                paramsStack.Add(param);
            }
        }

        public void TryRemoveParameter(string sessionId, IMacroParameter param) 
        {
            lock (m_Lock)
            {
                if (m_Parameters.TryGetValue(sessionId, out List<IMacroParameter> paramsStack))
                {
                    paramsStack.Remove(param);

                    if (!paramsStack.Any())
                    {
                        m_Parameters.Remove(sessionId);
                    }
                }
            }
        }
    }
}
