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
    [Guid("5CC2290A-E5E4-4B47-9A7F-A57FC619E27E")]
    public interface IArgumentsParameter : IMacroParameter
    {
        string[] Arguments { get; set; }
    }

    [ComVisible(true)]
    [Guid("860F96F7-4BC4-4927-AC8A-2C6176C7C0CB")]
    [ProgId("CadPlus.MacroRunner.ArgumentsParameter")]
    public class ArgumentsParameter : IArgumentsParameter
    {
        public string[] Arguments { get; set; }
        public IMacroResult Result { get; set; }

        public ArgumentsParameter()
        {
        }

        public ArgumentsParameter(string[] args) 
        {
            Arguments = args;
        }

        public ArgumentsParameter(string args) : this(new string[0])
        {
            //TODO: split args
        }
    }

    [ComVisible(true)]
    [Guid("51FD8EF9-505D-40B1-837A-E38A6F2FBA4A")]
    public interface IStatusResult : IMacroResult
    {
        bool Result { get; set; }
        string Message { get; set; }
    }

    [ComVisible(true)]
    [Guid("A72178E3-B5E5-4D04-8C35-0321CAD3C49A")]
    [ProgId("CadPlus.MacroRunner.StatusResult")]
    public class StatusResult : IStatusResult
    {
        public bool Result { get; set; }
        public string Message { get; set; }
    }

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

    [ComVisible(true)]
    [Guid("35C9ABF1-4C21-4810-B8C1-CB15394A13D1")]
    public interface IMacroRunner : IDisposable
    {
        IMacroResult Run(object appDisp, string macroPath, string moduleName, string subName, int opts, IMacroParameter param, bool cacheReg = false);
        IMacroParameter PopParameter(object appDisp);
    }
}
