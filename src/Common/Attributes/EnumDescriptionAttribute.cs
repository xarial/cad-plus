//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.ComponentModel;

namespace Xarial.CadPlus.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumDescriptionAttribute : DescriptionAttribute
    {
        public EnumDescriptionAttribute(string desc) : base(desc)
        {
        }
    }
}