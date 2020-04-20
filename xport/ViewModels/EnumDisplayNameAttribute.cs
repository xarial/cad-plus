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
    public class EnumDisplayNameAttribute : DescriptionAttribute 
    {
        public EnumDisplayNameAttribute(string dispName) : base(dispName)
        {
        }
    }
}
