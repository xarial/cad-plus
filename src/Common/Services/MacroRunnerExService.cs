using System;
using System.Linq;
using System.Runtime.InteropServices;
using Xarial.CadPlus.Common.Exceptions;
using Xarial.CadPlus.MacroRunner;
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Enums;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Structures;

namespace Xarial.CadPlus.Common.Services
{
    public interface IMacroRunnerExService
    {
        void RunMacro(IXApplication app, string macroPath, MacroEntryPoint entryPoint,
            MacroRunOptions_e opts, string args, IXDocument doc);
    }

    public abstract class MacroRunnerExService : IMacroRunnerExService, IDisposable
    {
        [DllImport("shell32.dll", SetLastError = true)]
        private static extern IntPtr CommandLineToArgvW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

        private readonly IMacroRunner m_Runner;

        public MacroRunnerExService()
        {
            m_Runner = TryCreateMacroRunner();
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
                            param.Set("Args", ParseCommandLine(args));
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
            catch (MacroUserInterruptException) //do not consider this as an error
            {
            }
            catch (MacroRunnerResultError resEx) 
            {
                throw new MacroRunFailedException(macroPath, -1, resEx.Message);
            }
        }

        protected abstract string MacroRunnerProgId { get; }
        protected abstract object GetDocumentDispatch(IXDocument doc);
        protected abstract object GetAppDispatch(IXApplication app);

        private string[] ParseCommandLine(string cmdLineArgs)
        {
            int count;
            var argsPtr = CommandLineToArgvW(cmdLineArgs, out count);

            if (argsPtr != IntPtr.Zero)
            {
                try
                {
                    var args = new string[count];

                    for (var i = 0; i < args.Length; i++)
                    {
                        var ptr = Marshal.ReadIntPtr(argsPtr, i * IntPtr.Size);
                        args[i] = Marshal.PtrToStringUni(ptr);
                    }

                    return args;
                }
                catch (Exception ex)
                {
                    throw new UserException("Failed to parse arguments", ex);
                }
                finally
                {
                    Marshal.FreeHGlobal(argsPtr);
                }
            }
            else
            {
                throw new Exception("Failed to parse arguments, pointer is null");
            }
        }

        public void Dispose()
        {
            m_Runner.Dispose();
        }
    }
}
