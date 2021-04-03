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
using Xarial.CadPlus.Plus.Exceptions;

namespace Xarial.CadPlus.Common.Utils
{
    public static class CommandLineHelper
    {
        [DllImport("shell32.dll", SetLastError = true)]
        private static extern IntPtr CommandLineToArgvW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

        public static string[] ParseCommandLine(string cmdLineArgs)
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
    }
}
