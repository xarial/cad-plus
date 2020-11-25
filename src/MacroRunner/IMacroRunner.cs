//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.MacroRunner
{
    [ComVisible(true)]
    [Guid("B2C8F829-9E75-4587-A1A2-CD3C02BA2CB9")]
    public interface IMacroParameter
    {
        IMacroResult Result { get; set; }
    }

    [ComVisible(true)]
    [Guid("77C12C96-9602-4D08-A22C-CB5C280BBDF1")]
    public interface IMacroResult 
    {
    }

    [ComVisible(true)]
    [Guid("1D7EB548-2651-4656-B205-31F258D81AF1")]
    public interface IMacroParameterManager
    {
        IMacroParameter PopParameter(string sessionId);
        void PushParameter(string sessionId, IMacroParameter param);
    }

    [ComVisible(true)]
    [Guid("CF4B61BF-7598-4189-9D5B-D96B7E2587D0")]
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

    [ComVisible(true)]
    [Guid("35C9ABF1-4C21-4810-B8C1-CB15394A13D1")]
    public interface IMacroRunner : IDisposable
    {
        IMacroResult Run(object appDisp, string macroPath, string moduleName, string subName, int opts, IMacroParameter param, bool cacheReg = false);
        IMacroParameter PopParameter(object appDisp);
    }
}
