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
using Xarial.CadPlus.Common.Exceptions;
using Xarial.CadPlus.Common.Utils;
using Xarial.CadPlus.MacroRunner;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.CadPlus.Plus.Services;
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Enums;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Structures;

namespace Xarial.CadPlus.Common.Services
{
    public abstract class MacroExecutor : IMacroExecutor, IDisposable
    {
        private readonly IMacroRunner m_Runner;
        private readonly IXCadMacroProvider m_XCadMacroProvider;
        private readonly Dictionary<string, IXCadMacro> m_XCadMacrosCache;
            
        public MacroExecutor(IXCadMacroProvider xCadMacroProvider)
        {
            m_Runner = TryCreateMacroRunner();
            m_XCadMacroProvider = xCadMacroProvider;
            m_XCadMacrosCache = new Dictionary<string, IXCadMacro>(StringComparer.CurrentCultureIgnoreCase);
        }

        private IMacroRunner TryCreateMacroRunner() 
        {
            try
            {
                return (IMacroRunner)Activator.CreateInstance(Type.GetTypeFromProgID(MacroRunnerProgId));
            }
            catch 
            {
                return null;
            }
        }

        public void RunMacro(IXApplication app, string macroPath, MacroEntryPoint entryPoint,
            MacroRunOptions_e opts, string args, IXDocument doc)
        {
            try
            {
                var argsArr = !string.IsNullOrEmpty(args) ? CommandLineHelper.ParseCommandLine(args) : null;

                var xCadMacro = GetXCadMacroIfExists(macroPath);

                if (xCadMacro == null)
                {
                    if (entryPoint == null)
                    {
                        var macro = app.OpenMacro(macroPath);

                        if (macro.EntryPoints != null)
                        {
                            entryPoint = macro.EntryPoints.First();
                        }
                        else
                        {
                            throw new MacroRunFailedException(macro.Path, -1, "Failed to extract entry point");
                        }
                    }

                    if (!string.IsNullOrEmpty(args) || doc != null)
                    {
                        if (m_Runner != null)
                        {
                            var param = new MacroParameter();

                            if (doc != null)
                            {
                                param.Set("Model", GetDocumentDispatch(doc));
                            }
                            if (!string.IsNullOrEmpty(args))
                            {
                                param.Set("Args", argsArr);
                            }

                            var res = m_Runner.Run(GetAppDispatch(app),
                                macroPath, entryPoint.ModuleName,
                                entryPoint.ProcedureName, (int)opts, param, true);

                            if (!res.Result)
                            {
                                throw new MacroRunnerResultError(res.Message);
                            }
                        }
                        else
                        {
                            throw new UserException("Macro runner is not installed. Cannot run the macro with arguments");
                        }
                    }
                    else
                    {
                        var macro = app.OpenMacro(macroPath);
                        macro.Run(entryPoint, opts);
                    }
                }
                else 
                {
                    xCadMacro.Run(app, doc, argsArr);
                }
            }
            catch (MacroUserInterruptException) //do not consider this as an error
            {
            }
            catch (MacroRunnerResultError resEx) 
            {
                throw new MacroRunFailedException(macroPath, -1, resEx.Message);
            }
        }

        private IXCadMacro GetXCadMacroIfExists(string path) 
        {
            IXCadMacro macro;

            if (m_XCadMacrosCache.TryGetValue(path, out macro)) 
            {
                return macro;
            }

            try
            {
                macro = m_XCadMacroProvider.GetMacro(path);
                m_XCadMacrosCache.Add(path, macro);
            }
            catch (NotXCadMacroDllException)
            {
                return null;
            }

            return macro;
        }

        protected abstract string MacroRunnerProgId { get; }
        protected abstract object GetDocumentDispatch(IXDocument doc);
        protected abstract object GetAppDispatch(IXApplication app);

        public void Dispose()
        {
            m_Runner.Dispose();
        }
    }
}
