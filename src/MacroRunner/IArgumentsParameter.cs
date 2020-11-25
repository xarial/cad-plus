//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Runtime.InteropServices;

namespace Xarial.CadPlus.MacroRunner
{
    [ComVisible(true)]
    [Guid("5CC2290A-E5E4-4B47-9A7F-A57FC619E27E")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IArgumentsParameter : IMacroParameter
    {
        [DispId(2)]
        object[] Arguments { get; set; }
        
        [DispId(3)]
        object Get(int index);

        [DispId(4)]
        int Count { get; }
    }

    [ComVisible(true)]
    [Guid("860F96F7-4BC4-4927-AC8A-2C6176C7C0CB")]
    [ProgId("CadPlus.MacroRunner.ArgumentsParameter")]
    public class ArgumentsParameter : IArgumentsParameter
    {
        public IMacroResult Result { get; set; }

        public int Count => Arguments.Length;

        public object[] Arguments { get; set; }

        public object Get(int index) => Arguments[index];

        public ArgumentsParameter()
        {
        }

        public ArgumentsParameter(params object[] args)
        {
            Arguments = args;
        }

        public ArgumentsParameter(string args) : this(new string[0])
        {
            //TODO: split args
        }
    }
}
