//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad;
using Xarial.XCad.Enums;
using Xarial.XCad.Structures;
using Xarial.XCad.Toolkit.Windows;

namespace Xarial.CadPlus.MacroRunner
{
    [ComVisible(true)]
    [Guid("9312AC25-2DCD-40C5-9F31-021865E09E87")]
    [ClassInterface(ClassInterfaceType.None)]
    public abstract class MacroRunnerBase : IMacroRunner
    {
        private const string MACRO_RUNNER_MONIKER_NAME = "_Xarial_CadPlus_MacroRunner_";

        private RotRegister m_Register;
        
        public void Dispose()
        {
            m_Register?.Dispose();
        }

        public IMacroParameter PopParameter(object appDisp)
        {
            try
            {
                var app = CastApplication(appDisp);

                GetMacroParametersManager(false, out _, out IMacroParameterManager macroParamsMgr);

                if (macroParamsMgr != null)
                {
                    var sessionId = GetCurrentMacroSessionId(app);
                    var param = macroParamsMgr.PopParameter(sessionId);
                    return param;
                }
                else
                {
                    throw new COMException("Failed to retrieve the macro parameters manager");
                }
            }
            catch (COMException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new COMException(ex.Message, ex.InnerException);
            }
        }

        public IMacroResult Run(object appDisp, string macroPath, string moduleName, 
            string subName, int opts, IMacroParameter param, bool cacheReg = false)
        {
            try
            {
                var app = CastApplication(appDisp);

                var macro = app.OpenMacro(macroPath);

                GetMacroParametersManager(true, out RotRegister newReg, out IMacroParameterManager macroParamsMgr);

                if (newReg != null)
                {
                    if (cacheReg)
                    {
                        if (m_Register != null)
                        {
                            try
                            {
                                m_Register.Dispose();
                            }
                            catch
                            {
                            }
                        }

                        m_Register = newReg;
                    }
                }

                try
                {
                    var sessionId = CreateMacroSessionId(app, macro);
                    macroParamsMgr.PushParameter(sessionId, param);

                    try
                    {
                        macro.Run(new MacroEntryPoint(moduleName, subName), (MacroRunOptions_e)opts);
                    }
                    finally
                    {
                        macroParamsMgr.TryRemoveParameter(sessionId, param);
                    }
                }
                finally
                {
                    if (newReg != null && !cacheReg)
                    {
                        newReg.Dispose();
                    }
                }

                return param.Result;
            }
            catch (COMException) 
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new COMException(ex.Message, ex.InnerException);
            }
        }

        protected abstract IXApplication CastApplication(object app);
        protected abstract string CreateMacroSessionId(IXApplication app, IXMacro macro);
        protected abstract string GetCurrentMacroSessionId(IXApplication app);

        private void GetMacroParametersManager(bool createIfNotExist, out RotRegister newRegister, out IMacroParameterManager macroParamsMgr) 
        {
            macroParamsMgr = RotHelper.TryGetComObjectByMonikerName<IMacroParameterManager>(MACRO_RUNNER_MONIKER_NAME);

            if (macroParamsMgr == null)
            {
                if (createIfNotExist)
                {
                    macroParamsMgr = new MacroParameterManager();
                    newRegister = new RotRegister(macroParamsMgr, MACRO_RUNNER_MONIKER_NAME);
                }
                else
                {
                    throw new COMException("Macro parameters manager is not registered");
                }
            }
            else 
            {
                newRegister = null;
            }
        }
    }
}
