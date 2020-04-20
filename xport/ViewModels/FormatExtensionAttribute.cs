//*********************************************************************
//xTools
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://xtools.xarial.com
//License: https://xtools.xarial.com/license/
//*********************************************************************

using System;
using System.ComponentModel;

namespace Xarial.XTools.Xport.ViewModels
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
