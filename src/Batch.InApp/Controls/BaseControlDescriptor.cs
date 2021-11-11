//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.UI;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.UI;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;

namespace Xarial.CadPlus.Batch.InApp.Controls
{
    public class BaseControlDescriptor<TContext, TValue, TRibbonCmd> : IControlDescriptor
        where TRibbonCmd : IRibbonCommand
    {
        public string DisplayName { get; }
        public string Description { get; }
        public string Name { get; }

        public IXImage Icon { get; }
        public Type DataType { get; }
        public IAttribute[] Attributes { get; }

        private readonly Func<TContext, TValue> m_Getter;
        private readonly Action<TContext, TValue> m_Setter;

        public object GetValue(object context)
            => m_Getter.Invoke((TContext)context);

        public void SetValue(object context, object value)
            => m_Setter.Invoke((TContext)context, (TValue)value);

        public TRibbonCmd Command { get; }

        private static IAttribute[] GetAttributes(TRibbonCmd cmd) 
        {
            var atts = new List<IAttribute>();

            if (cmd.Icon != null)
            {
                //TODO: add icon attribute
                //atts.Add(new IconAttribute())
            }
            else 
            {
                atts.Add(new ControlOptionsAttribute(align: ControlLeftAlign_e.Indent));
            }

            return atts.ToArray();
        }

        public BaseControlDescriptor(TRibbonCmd cmd,
            Func<TContext, TValue> getter, Action<TContext, TValue> setter) 
            : this(cmd, GetAttributes(cmd), getter, setter)
        {
        }

        protected BaseControlDescriptor(TRibbonCmd cmd, IAttribute[] atts,
            Func<TContext, TValue> getter, Action<TContext, TValue> setter)
        {
            Command = cmd;

            DisplayName = cmd.Title;
            Description = cmd.Description;
            Name = Guid.NewGuid().ToString();
            Attributes = atts;
            DataType = typeof(TValue);

            m_Getter = getter;
            m_Setter = setter;
        }
    }
}
