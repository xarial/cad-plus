//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.ComponentModel;

namespace Xarial.CadPlus.Xport.ViewModels
{
    [AttributeUsage(AttributeTargets.Field)]
    public class FormatExtensionAttribute : DisplayNameAttribute
    {
        public string Extension { get; }

        public FormatExtensionAttribute(string ext) : base(ext)
        {
            Extension = ext;
        }
    }
}